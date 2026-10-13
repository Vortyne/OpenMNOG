#version 4
// (OpenMNOG: arriving with no background sound (a save made after a warp, or after the Chronicle once lost it), Ga-Koro's
// own: the water, which carries its music, and the village's sounds, as the ways in set them.)
if (getProperty("/Sound-Background", _url) eq "blank.swf") {
    tellTarget ("/") {
        Sound_Background = "SFX_WaterAmbiant.swf";
        Sound_ChangeBackgroundMovie = "0";
        Sound_SFXMovie = "Sound_GaKoro.swf";
        Sound_ChangeSFXMovie = "0";
    }
    loadMovie("SFX_WaterAmbiant.swf", "/Sound-Background");
    loadMovie("Sound_GaKoro.swf", "/Sound-SFX");
}
if (eval("/:fGaKoro") eq "10") {
    tellTarget ("Middle-Ground/Fortifications") {
        gotoAndStop(2);
    }
    tellTarget ("Back-Ground/Fortifications") {
        gotoAndStop(2);
    }
}
GaKoro = eval("/:GaKoro");
if (GaKoro eq "1") {
    setProperty("/holder/Back-Ground/Steam1", _y, "2000");
}
tellTarget ("/") {
    Startedloading = "1";
}
Rollover1Playing = "0";
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
if (eval("/:cKapura") eq "0" and eval("/:fGaKoro") eq "10" and eval("/:fTaKoro") eq "10" and eval("/:fOnuKoro") eq "10" and eval("/:fPoKoro") eq "10" and eval("/:fLeKoro") eq "10" and eval("/:fKoKoro") eq "10") {
} else {
    setProperty("Middle-Ground/Kapura", _x, "2000");
}
