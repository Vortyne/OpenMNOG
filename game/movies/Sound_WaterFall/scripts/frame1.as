#version 4
Sound_BG_Volume = eval("/:Sound_BG_Volume");
Sound_ChangeBackgroundVolume = "0";
tellTarget ("../") {
    BGS_loaded = "1";
}
if (Sound_BG_Volume eq "High") {
    gotoAndPlay(2);
} else if (Sound_BG_Volume eq "Medium") {
    gotoAndPlay(6);
} else if (Sound_BG_Volume eq "Low") {
    gotoAndPlay(10);
} else {
    stop();
}
