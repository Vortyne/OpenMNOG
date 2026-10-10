#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        MovieName = "XX.4-N.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "blank.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "Music_Monks.swf";
        KoKoro = "7";
        ShiftOff = "0";
        ShowBackPack = "1";
        BeginLoadMovie = "1";
    }
}
stop();
