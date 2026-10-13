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
GaKoro = eval("/:GaKoro");
if (GaKoro eq "1") {
    gotoAndPlay(3);
}
tellTarget ("/") {
    Startedloading = "1";
}
tellTarget ("/holder/Middle-Ground/Light2/Lightfish") {
    gotoAndPlay(30);
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
