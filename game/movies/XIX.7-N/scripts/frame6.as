#version 4
if (DragOn eq "1") {
    setProperty(Dragging, _x, getProperty("/MouseDrag", _x));
    yMinHeight = eval(Dragging add "/:yPosition");
    if (getProperty("/MouseDrag", _y) < yMinHeight) {
        setProperty(Dragging, _y, getProperty("/MouseDrag", _y));
    } else {
        setProperty(Dragging, _y, yMinHeight);
    }
}
if (Releasing eq "1") {
    count = "6";
    while (count < NumHoles) {
        count = count + "1";
        PlacerName = "Hole" add count add "/Placer";
        xPlacer = getProperty(PlacerName, _x) + eval("Hole" add count add "/:xOffset");
        yPlacer = getProperty(PlacerName, _y) + eval("Hole" add count add "/:yOffset");
        xDrag = getProperty(Dragging, _x);
        yDrag = getProperty(Dragging, _y);
        if (eval("Hole" add count add "/:Holding") eq "0") {
            if (xDrag < xPlacer + "20" and xPlacer - "20" < xDrag and yDrag < yPlacer + "20" and yPlacer - "20" < yDrag) {
                DragOn = "0";
                setProperty("Placed" add DragLevel add "-" add DragItem, _x, xPlacer);
                setProperty("Placed" add DragLevel add "-" add DragItem, _y, yPlacer);
                setProperty(Dragging, _x, "2000");
                tellTarget ("PlaceSound") {
                    gotoAndPlay("play");
                }
                set("Hole" add count add "/:Holding", "1");
                tellTarget (Dragging) {
                    Gravity = "0";
                }
                pLevelNum = eval("Hole" add count add "/:LevelNum");
                pItemNum = eval("Hole" add count add "/:ItemNum");
                dLevelNum = eval(Dragging add "/:LevelNum");
                dItemNum = eval(Dragging add "/:ItemNum");
                set("Hole" add count add "/:LevelHolding", dLevelNum);
                set("Hole" add count add "/:ItemHolding", dItemNum);
                if (pItemNum eq dItemNum and pLevelNum eq dLevelNum) {
                    set("Placed-" add pLevelNum add "-" add pItemNum, "1");
                }
            }
        }
    }
    Releasing = "0";
}
if (Pickup eq "1") {
    setProperty("Placed" add DragLevel add "-" add DragItem, _x, "2000");
    count = "6";
    while (count < NumHoles) {
        count = count + "1";
        PlacerName = "Hole" add count add "/Placer";
        xPlacer = getProperty(PlacerName, _x) + eval("Hole" add count add "/:xOffset");
        yPlacer = getProperty(PlacerName, _y) + eval("Hole" add count add "/:yOffset");
        xDrag = getProperty(Dragging, _x);
        yDrag = getProperty(Dragging, _y);
        if (eval("Hole" add count add "/:Holding") eq "1") {
            if (xDrag < xPlacer + "50" and xPlacer - "50" < xDrag and yDrag < yPlacer + "50" and yPlacer - "50" < yDrag) {
                pLevelHolding = eval("Hole" add count add "/:LevelHolding");
                pItemHolding = eval("Hole" add count add "/:ItemHolding");
                dLevelNum = eval(Dragging add "/:LevelNum");
                dItemNum = eval(Dragging add "/:ItemNum");
                if (pLevelHolding eq dLevelNum and pItemHolding eq dItemNum) {
                    set("Hole" add count add "/:Holding", "0");
                    pLevelNum = eval("Hole" add count add "/:LevelNum");
                    pItemNum = eval("Hole" add count add "/:ItemNum");
                    set("Placed-" add pLevelNum add "-" add pItemNum, "0");
                }
            }
        }
    }
    Pickup = "0";
}
count = "0";
while (count < "6") {
    count = count + "1";
    GravCheck = eval("Level1-" add count add "/:Gravity");
    if ("0" < GravCheck) {
        yFallTo = eval("Level1-" add count add "/:yPosition");
        yHeight = getProperty("Level1-" add count, _y);
        if (yFallTo < yHeight + GravCheck) {
            tellTarget ("Level1-" add count) {
                Gravity = "0";
            }
            setProperty("Level1-" add count, _y, yFallTo);
            tellTarget ("LandSound") {
                gotoAndPlay("play");
            }
        } else {
            tellTarget ("Level1-" add count) {
                Gravity = Gravity + "1";
            }
            setProperty("Level1-" add count, _y, getProperty("Level1-" add count, _y) + GravCheck);
        }
    }
}
count = "0";
while (count < "6") {
    count = count + "1";
    GravCheck = eval("Level2-" add count add "/:Gravity");
    if ("0" < GravCheck) {
        yFallTo = eval("Level2-" add count add "/:yPosition");
        yHeight = getProperty("Level2-" add count, _y);
        if (yFallTo < yHeight + GravCheck) {
            tellTarget ("Level2-" add count) {
                Gravity = "0";
            }
            setProperty("Level2-" add count, _y, yFallTo);
            tellTarget ("LandSound") {
                gotoAndPlay("play");
            }
        } else {
            tellTarget ("Level2-" add count) {
                Gravity = Gravity + "1";
            }
            setProperty("Level2-" add count, _y, getProperty("Level2-" add count, _y) + GravCheck);
        }
    }
}
