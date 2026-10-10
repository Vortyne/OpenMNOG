#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        MovieName = "XVIII.5-S.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Amb_Jungle.swf";
        ShiftOff = "0";
        LeKoro = "3";
        BeginLoadMovie = "1";
    }
}
stop();
