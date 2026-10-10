#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else {
    setTarget("/");
    MovieName = "XVIII.4-S.swf";
    Sound_ChangeBackgroundMovie = "1";
    Sound_Background = "Amb_Jungle.swf";
    Sound_ChangeSFXMovie = "1";
    Sound_SFXMovie = eval("blank:swf");
    Music_ChangeMusic = "1";
    Music_Movie = "Music_band.swf";
    ShiftOff = "0";
    HorizontalShift = "1";
    VerticalShift = "0";
    LeKoro = "5";
    if (Taipu eq "Caught") {
        Taipu = "Free";
    }
    BeginLoadMovie = "1";
    setTarget("");
}
stop();
