#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        MovieName = "V.4-S.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "Sound_GaKoro.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "SFX_WaterAmbiant.swf";
        VerticalShift = "0";
        HorizontalShift = "1";
        ShiftOff = "0";
        GaKoro = "1";
        BeginLoadMovie = "1";
    }
}
stop();
