#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        MovieName = "Game_6-1.swf";
        ShiftOff = "1";
        LeKoro = "4";
        BeginLoadMovie = "1";
    }
}
stop();
