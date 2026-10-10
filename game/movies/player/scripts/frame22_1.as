#version 5
#operators flash4
currenttime = getTimer();
if (fGaKoro eq "10" and fTaKoro eq "10" and fLeKoro eq "10" and fPoKoro eq "10" and fOnuKoro eq "10") {
    fKoKoro = "10";
}
if (Backpack eq "1" and (substring(Moviename, "0", "7") eq "Episode" or substring(Moviename, "0", "4") eq "Game" or BookOpen eq "1" or ShowBackPack eq "0")) {
    setProperty("Backpack", _visible, "0");
} else {
    setProperty("Backpack", _visible, "1");
}
if (LoadedBackpack != "1" and Backpack eq "1") {
    loadMovie("backpack.swf", "Backpack");
    LoadedBackPack = "1";
}
if (LoadedFlute eq "0" and Flute eq "1") {
    loadMovie("flute.swf", "Flute");
    LoadedFlute = "1";
}
if (Flute eq "1") {
    if (LoadedFlute eq "1" and FluteOpen eq "1") {
        if (getProperty("Flute", _x) eq "2000") {
            tellTarget ("Backpack/Backpack") {
                gotoAndPlay("ResetOrder");
            }
        }
        setProperty("Flute", _x, "0");
        FluteNoShift = "1";
    } else {
        setProperty("Flute", _x, "2000");
        tellTarget ("Flute/Flute/Flute/correct") {
            gotoAndPlay("idle");
        }
        FluteNoShift = "0";
    }
}
_level0.__matanui.panScene();
setProperty("/holder/Buttons", _x, getProperty("/holder/Middle-Ground", _x));
setProperty("/holder/Buttons", _y, getProperty("/holder/Middle-Ground", _y));
time = getTimer();
framecount = framecount + "1";
if ("100" < time - lasttime) {
    framerate = framecount * "1000" / (time - lasttime);
    framecount = "0";
    lasttime = getTimer();
}
DebugFGH = getProperty("/holder/Fore-Ground", _x);
DebugMGH = getProperty("/holder/Middle-Ground", _x);
DebugBGH = getProperty("/holder/Back-Ground", _x);
DebugFGV = getProperty("/holder/Fore-Ground", _y);
DebugMGV = getProperty("/holder/Middle-Ground", _y);
DebugBGV = getProperty("/holder/Back-Ground", _y);
_level0.__matanui.clampPanorama();
// (Port: a rope bridge sized to the layers' positions (XVIII.3-S) is sized again now they've moved this frame, so
// it doesn't trail them by a frame and catch up in jumps.)
if (eval("/holder/Fore-Ground/Bridge/:Lwidth") ne "") {
    DebugFGH = getProperty("/holder/Fore-Ground", _x);
    DebugMGH = getProperty("/holder/Middle-Ground", _x);
    call("/holder/Fore-Ground:1");
    call("/holder/Fore-Ground/Bridge:2");
    call("/holder/Fore-Ground/Bridge/Planks:2");
}
