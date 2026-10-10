#version 4
count = "0";
BasketHeight = BasketBaseHeight + DisplacedHeight * "2";
CWHeight = CWBaseHeight - DisplacedHeight * "2";
DialRotation = "-155" + "5" * DisplacedHeight;
if (getProperty("Dial", _rotation) < DialRotation - "0.5") {
    setProperty("Dial", _rotation, getProperty("Dial", _rotation) + "5");
} else if (DialRotation + "0.5" < getProperty("Dial", _rotation)) {
    setProperty("Dial", _rotation, getProperty("Dial", _rotation) - "5");
}
setProperty("Dial", _xscale, "100");
setProperty("Dial", _yscale, "100");
if (DisplacedHeight eq "31" and GateIsOpen eq "0") {
    GateIsOpen = "1";
    tellTarget ("Gate") {
        gotoAndPlay("Rise");
    }
}
DisplacedHeight = "0";
while (count < NumRocks) {
    count = count + "1";
    if ("0" < eval("PlacedRock" add count)) {
        if (eval("DonePlacing" add count) != "1") {
            Acceleration = eval("RockAccel" add count);
            if (getProperty("PlacedRock" add count, _y) < getProperty("BasketFront", _y) - "17" - Acceleration) {
                setProperty("PlacedRock" add count, _y, getProperty("PlacedRock" add count, _y) + Acceleration);
                set("RockAccel" add count, eval("RockAccel" add count) + "1.5");
            } else if (getProperty("PlacedRock" add count, _y) < getProperty("BasketFront", _y) - "17" - "0.5" * Acceleration) {
                setProperty("PlacedRock" add count, _y, getProperty("PlacedRock" add count, _y) + "0.5" * Acceleration);
                set("RockAccel" add count, eval("RockAccel" add count) + "1.5");
            } else {
                NumPlaced = NumPlaced + "1";
                BasketAccel = BasketAccel + "1" / (NumPlaced + "20") * Acceleration;
                CWAccel = CWAccel - "1" / (NumPlaced + "20") * Acceleration;
                set("DonePlacing" add count, "1");
            }
        } else {
            DisplacedHeight = DisplacedHeight + eval("PlacedRock" add count add "/:Displacement");
        }
    }
}
count = "0";
while (count < NumRocks) {
    count = count + "1";
    if ("0" < eval("PlacedRock" add count) and eval("DonePlacing" add count) eq "1") {
        setProperty("PlacedRock" add count, _y, getProperty("PlacedRock" add count, _y) + BasketAccel);
    }
}
setProperty("BasketFront", _y, getProperty("BasketFront", _y) + BasketAccel);
setProperty("BasketBack", _y, getProperty("BasketBack", _y) + BasketAccel);
setProperty("CounterWeight", _y, getProperty("CounterWeight", _y) + CWAccel);
BasketCurrentHeight = getProperty("BasketFront", _y);
if (BasketHeight < getProperty("BasketFront", _y)) {
    BasketAccel = BasketAccel - "0.05" * (BasketCurrentHeight - BasketHeight);
} else if (getProperty("BasketFront", _y) < BasketHeight) {
    BasketAccel = BasketAccel - "0.05" * (BasketCurrentHeight - BasketHeight);
}
BasketAccel = BasketAccel * "0.9";
CWCurrentHeight = getProperty("CounterWeight", _y);
if (CWHeight < getProperty("CounterWeight", _y)) {
    CWAccel = CWAccel - "0.05" * (CWCurrentHeight - CWHeight);
} else if (getProperty("CounterWeight", _y) < CWHeight) {
    CWAccel = CWAccel - "0.05" * (CWCurrentHeight - CWHeight);
}
CWAccel = CWAccel * "0.9";
count = "0";
while (count < NumRocks) {
    count = count + "1";
    if (getProperty("Rock" add count, _y) < eval("StartHeightRock" add count) - eval("RockAccel" add count)) {
        if (Holding eq "1" and count eq WhichChecking) {
            set("RockAccel" add count, "0");
        } else {
            setProperty("Rock" add count, _y, getProperty("Rock" add count, _y) + eval("RockAccel" add count));
            set("RockAccel" add count, eval("RockAccel" add count) + "1.5");
        }
        if (eval("../Rock" add count add "/:PickedUp") eq "0") {
        }
    } else if (getProperty("Rock" add count, _y) < eval("StartHeightRock" add count) and eval("StartHeightRock" add count) - eval("RockAccel" add count) < getProperty("Rock" add count, _y)) {
        setProperty("Rock" add count, _y, eval("StartHeightRock" add count));
        RandNum = random("2000");
        duplicateMovieClip("ImpactBase", "Impact" add RandNum, "16384" + ImpactCount);
        setProperty("Impact" add RandNum, _y, getProperty("Rock" add count, _y) + "13.2");
        setProperty("Impact" add RandNum, _x, getProperty("Rock" add count, _x));
        setProperty("Impact" add RandNum, _xscale, eval("RockAccel" add count) * "1.5" + "90");
        setProperty("Impact" add RandNum, _yscale, eval("RockAccel" add count) * "1.2" + "100");
        ImpactCount = ImpactCount + "1";
        if ("100" < ImpactCount) {
            ImpactCount = "20";
        }
        set("RockAccel" add count, "0");
    } else {
        if (Holding != "1") {
            setProperty("Rock" add count, _y, eval("StartHeightRock" add count));
        }
        set("RockAccel" add count, "0");
    }
}
