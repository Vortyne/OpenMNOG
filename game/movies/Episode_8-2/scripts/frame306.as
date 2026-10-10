#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        Quest = "2";
        MovieName = "XXII.6-N.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Amb_Jungle.swf";
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
