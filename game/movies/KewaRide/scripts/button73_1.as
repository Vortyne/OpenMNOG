#version 4
if (eval("/:KoKoro") < "7") {
    tellTarget ("../") {
        gotoAndPlay("Storm");
    }
} else {
    tellTarget ("/") {
        MovieName = "XX.2-N.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Amb_rockwind.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "Sound_MntGust.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        ShowBackPack = "1";
        VerticalShift = "0";
        HorizontalShift = "1";
        ShiftOff = "0";
        BeginLoadMovie = "1";
    }
}
