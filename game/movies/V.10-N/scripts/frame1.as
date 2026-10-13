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
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:cMaku") eq "1") {
    setProperty("Middle-Ground/MakuPickup", _x, "2000");
    setProperty("Middle-Ground/MakuBubble", _x, "2000");
    setProperty("Middle-Ground/Maku", _x, "2000");
} else if (eval("/:cKapura") eq "1") {
    setProperty("Middle-Ground/MakuBubble", _x, "2000");
} else {
    setProperty("Middle-Ground/MakuPickup", _x, "2000");
}
