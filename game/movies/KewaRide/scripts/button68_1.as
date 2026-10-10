#version 4
if (eval("/:fLeKoro") eq "10") {
    tellTarget ("/") {
        Music_ChangeMusic = "1";
        Music_Movie = "Music_Jungle.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "Sound_Jungle.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Amb_Jungle.swf";
        MovieName = "XVIII.1-S.swf";
        ShowBackPack = "1";
        VerticalShift = "1";
        HorizontalShift = "0";
        ShiftOff = "0";
        BeginLoadMovie = "1";
    }
} else {
    if ("4" < eval("/:LeKoro")) {
        tellTarget ("/") {
            Music_ChangeMusic = "1";
            Music_Movie = "Music_band.swf";
            Sound_ChangeSFXMovie = "1";
            Sound_SFXMovie = "blank.swf";
            Sound_ChangeBackgroundMovie = "1";
            Sound_Background = "Amb_Jungle.swf";
        }
    } else {
        tellTarget ("/") {
            Music_ChangeMusic = "1";
            Music_Movie = "Music_Jungle.swf";
            Sound_ChangeSFXMovie = "1";
            Sound_SFXMovie = "Sound_Jungle.swf";
            Sound_ChangeBackgroundMovie = "1";
            Sound_Background = "Amb_Jungle.swf";
        }
    }
    tellTarget ("/") {
        MovieName = "XVIII.5-S.swf";
        ShowBackPack = "1";
        VerticalShift = "0";
        HorizontalShift = "1";
        ShiftOff = "0";
        BeginLoadMovie = "1";
    }
}
