#version 4
SnowRandom = random("4") + "1";
if (SnowRandom eq "1") {
    tellTarget ("Snow_FG") {
        gotoAndStop(1);
    }
} else if (SnowRandom eq "2") {
    tellTarget ("Snow_FG") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Flag") {
        gotoAndStop(3);
    }
    tellTarget ("Fore-Ground/Flag") {
        gotoAndStop(2);
    }
} else if (SnowRandom eq "3") {
    tellTarget ("Snow_FG") {
        gotoAndStop(3);
    }
} else if (SnowRandom eq "4") {
    tellTarget ("Snow_FG") {
        gotoAndStop(4);
    }
    tellTarget ("Middle-Ground/Flag") {
        gotoAndStop(3);
    }
    tellTarget ("Fore-Ground/Flag") {
        gotoAndStop(2);
    }
}
if (eval("/:FGFLag") eq "1") {
    FlagRandomX = random("400") + "1";
    RandomNeg = random("2") + "1";
    if (RandomNeg eq "1") {
        FlagRandomX = FlagRandomX * "-1";
    }
    setProperty("Fore-Ground/Flag", _x, FlagRandomX);
    setProperty("Fore-Ground/Flag", _y, "208");
} else {
    set("/:Flag", "5");
    if (eval("/:Flag")) {
    }
    FlagRandom = random("2") + "1";
    if (FlagRandom eq "1") {
        FlagRandomX = random("700") + "1";
        RandomNeg = random("2") + "1";
        if (RandomNeg eq "1") {
            FlagRandomX = FlagRandomX * "-1";
        }
        setProperty("Middle-Ground/Flag", _x, FlagRandomX);
        setProperty("Middle-Ground/Flag", _y, "110");
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
