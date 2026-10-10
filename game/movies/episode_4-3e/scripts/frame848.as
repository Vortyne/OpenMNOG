#version 4
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        MovieName = "IX.4-E.swf";
        VerticalShift = "0";
        HorizontalShift = "1";
        ShiftOff = "0";
        ShowBackPack = "1";
        PoKoro = "1";
        BeginLoadMovie = "1";
    }
}
stop();
