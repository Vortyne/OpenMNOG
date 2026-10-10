#version 4
setTarget("/");
if (eval("/:GaKoro") eq "1") {
    MovieName = "V.12-N.swf";
    Sound_ChangeSFXMovie = "1";
    Sound_SFXMovie = "Sound_GaKoro.swf";
    Sound_ChangeBackgroundMovie = "1";
    Sound_Background = "SFX_WaterAmbiant.swf";
    ShiftOff = "1";
} else {
    MovieName = "V.1-S.swf";
    Sound_ChangeSFXMovie = "1";
    Sound_SFXMovie = "Sound_Beach.swf";
    Sound_ChangeBackgroundMovie = "1";
    Sound_Background = "Sound_Waves.swf";
    Sound_ChangeBackgroundVolume = "1";
    Sound_BG_Volume = "Low";
    ShiftOff = "0";
    GaKoroVisited = "1";
    ScreenPositionChangeH = "1";
    ScreenPositionFGH = "-312";
    ScreenPositionMGH = "30";
    ScreenPositionBGH = "207.5";
}
BeginLoadMovie = "1";
setTarget("");
stop();
