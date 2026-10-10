#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    set("/:MovieName", "XXIV." add eval("/:FightScreen") add ".swf");
    tellTarget ("/") {
        VerticalShift = "0";
        HorizontalShift = "1";
        ShiftOff = "0";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "blank.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        BeginLoadMovie = "1";
    }
}
stop();
