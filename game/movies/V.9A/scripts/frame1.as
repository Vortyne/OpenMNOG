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
Count = "0";
Language = eval("/:Language");
GaKoro = eval("/:GaKoro");
TextToLoad = "Text_V.9A_1_" add Language add ".txt";
