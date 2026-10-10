#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        Quest = "3";
        MovieName = "XXII.9-N.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Amb_MntCaves.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        ShiftOff = "0";
        VerticalShift = "1";
        HorizontalShift = "0";
        ScreenPositionChangeV = "1";
        ScreenPositionFGV = "0";
        ScreenPositionMGV = "22.4";
        ScreenPositionBGV = "157.5";
        BeginLoadMovie = "1";
    }
}
stop();
