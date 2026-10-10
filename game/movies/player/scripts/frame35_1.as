#version 5
if (ScreenPositionChangeH eq "1") {
    ScreenPositionChangeH = "0";
    ScreenPositionChangeV = "0";
    setProperty("/holder/Fore-Ground", _x, ScreenPositionFGH);
    setProperty("/holder/Middle-Ground", _x, ScreenPositionMGH);
    setProperty("/holder/Back-Ground", _x, ScreenPositionBGH);
} else if (ScreenPositionChangeV eq "1") {
    ScreenPositionChangeH = "0";
    ScreenPositionChangeV = "0";
    setProperty("/holder/Fore-Ground", _y, ScreenPositionFGV);
    setProperty("/holder/Middle-Ground", _y, ScreenPositionMGV);
    setProperty("/holder/Back-Ground", _y, ScreenPositionBGV);
}
// (Port: the carried-over position kept inside the widescreen pan limits from the start, not shown past them and
// snapped back once the panning starts.)
_level0.__matanui.clampPanorama();
if (Backpack eq "1" and (substring(Moviename, "0", "7") eq "Episode" or substring(Moviename, "0", "4") eq "Game" or BookOpen eq "1" or ShowBackPack eq "0")) {
    setProperty("Backpack", _visible, "0");
} else {
    setProperty("Backpack", _visible, "1");
}
if (Backpack eq "1") {
    tellTarget ("/Backpack") {
        gotoAndPlay("Closed");
    }
    tellTarget ("/Backpack/cBar") {
        gotoAndPlay("Closed");
    }
}
if (eval("_level0:SkipLoadScreen") eq "1") {
    gotoAndPlay(43);
}
