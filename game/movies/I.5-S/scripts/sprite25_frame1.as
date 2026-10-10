#version 4
Sound_SFX = eval("/:Sound_SFX");
if (Sound_SFX eq "SFX_SeaSpray2.swf") {
    if (PlayedOnce eq "0") {
        PlayedOnce = "1";
        gotoAndPlay(3);
    }
} else {
    PlayedOnce = "0";
}
