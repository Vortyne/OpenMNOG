#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        MovieName = "XVIII.7.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Amb_hive.swf";
        ShiftOff = "0";
        VerticalShift = "0";
        HorizontalShift = "1";
        BeginLoadMovie = "1";
    }
}
stop();
