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
if (eval("/:FGFlag") eq "1") {
    FlagRandomX = random("400") + "1";
    RandomNeg = random("2") + "1";
    if (RandomNeg eq "1") {
        FlagRandomX = FlagRandomX * "-1";
    }
    setProperty("Fore-Ground/Flag", _x, FlagRandomX);
    setProperty("Fore-Ground/Flag", _y, "208");
} else if (eval("/:FGShrine") eq "1") {
    setProperty("Fore-Ground/Shrine", _y, "32");
    setProperty("Middle-Ground/BG1", _y, "-1000");
    setProperty("Back-Ground/BG2", _y, "-1000");
    setProperty("Middle-Ground/BG3", _y, "-1000");
} else if (eval("/:FGShrine") eq "2") {
    tellTarget ("Fore-Ground/Shrine") {
        gotoAndStop(2);
    }
    setProperty("Fore-Ground/Shrine", _y, "32");
    setProperty("Middle-Ground/BG1", _y, "-1000");
    setProperty("Back-Ground/BG2", _y, "-1000");
    setProperty("Middle-Ground/BG3", _y, "-1000");
} else if (eval("/:Flag") eq "3") {
    setProperty("Middle-Ground/Shrine", _y, "0");
    set("/:BGShrine", "1");
    setProperty("Middle-Ground/BG1", _y, "-1000");
    setProperty("Back-Ground/BG2", _y, "-1000");
    setProperty("Middle-Ground/BG3", _y, "-1000");
} else {
    FlagRandom = random("2") + "1";
    if (FlagRandom eq "1") {
        FlagRandomX = random("650") + "1";
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
