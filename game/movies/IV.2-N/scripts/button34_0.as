#version 4
IV2Back = eval("/:IV2Back");
if (IV2Back eq "IV.4-N.swf") {
    tellTarget ("/") {
        HorizontalShift = "0";
        VerticalShift = "1";
        Sound_ChangeBackgroundVolume = "1";
        Sound_BG_Volume = "High";
    }
} else {
    tellTarget ("/") {
        IV2Back = "IV.4-S.swf";
        Sound_ChangeBackgroundVolume = "1";
        Sound_BG_Volume = "Low";
    }
}
tellTarget ("/") {
    Sound_ChangeSFXMovie = "1";
    Sound_SFXMovie = "blank.swf";
    MovieName = IV2Back;
    BeginLoadMovie = "1";
}
