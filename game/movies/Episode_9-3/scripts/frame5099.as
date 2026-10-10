#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        MovieName = "I.1-S.swf";
        ShowBackPack = "0";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Amb_Waves.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        ShiftOff = "0";
        VerticalShift = "0";
        HorizontalShift = "1";
        BeginLoadMovie = "1";
    }
}
stop();
