#version 4
tellTarget ("/") {
    MovieName = "XI.1-S.swf";
    Sound_ChangeBackgroundMovie = "1";
    Sound_Background = "blank.swf";
    Sound_ChangeSFXMovie = "1";
    Sound_SFXMovie = "blank.swf";
    Music_ChangeMusic = "1";
    // (OpenMNOG: Onu-Koro's second music only once the lava is fixed; before that, its first.)
    if (eval("/:OnuKoroLava") eq "1") {
        Music_Movie = "Music_OnuKoro_2.swf";
    } else {
        Music_Movie = "Music_OnuKoro.swf";
    }
    ShiftOff = "0";
    VerticalShift = "0";
    HorizontalShift = "1";
    BeginLoadMovie = "1";
}
stop();
