#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        Quest = "1";
        MovieName = "XXII.1-N.swf";
        ShiftOff = "0";
        VerticalShift = "1";
        HorizontalShift = "0";
        ScreenPositionChangeV = "1";
        ScreenPositionFGV = "0";
        ScreenPositionMGV = "22.4";
        ScreenPositionBGV = "157.5";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Sound_Waterfall.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        BeginLoadMovie = "1";
    }
}
stop();
