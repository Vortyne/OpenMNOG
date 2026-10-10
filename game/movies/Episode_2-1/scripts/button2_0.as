#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        TaVisit = "1";
        MovieName = "IV.6A.swf";
        VerticalShift = "0";
        HorizontalShift = "0";
        Sound_ChangeBackgroundMovie = "1";
        Sound_BackGround = "Sound_Vakama.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        BeginLoadMovie = "1";
    }
}
