#version 4
Dragging = "-1";
setProperty(Slot1, _x, "-306.1");
setProperty(Slot1, _y, "-9.3");
if (Slot1 eq "Book") {
    tellTarget ("/") {
        BookOpen = "1";
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else if (Slot1 eq "PlanBook") {
    tellTarget ("/") {
        BookOpen = "1";
        BookMovieName = "PlanBook.swf";
        BeginLoadMovie = "1";
    }
} else if (Slot1 eq "Flute") {
    tellTarget ("/") {
        loadMovie("blank.swf", "Music-holder");
        FluteOpen = "1";
    }
}
