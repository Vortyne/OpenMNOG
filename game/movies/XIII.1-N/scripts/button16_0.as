#version 4
tellTarget ("/") {
    MovieName = "XI.8-N.swf";
    if (eval("/:OnuKoroLava") eq "1") {
        // (OpenMNOG: Onu-Koro's second music once the lava is fixed, carrying on if already playing.)
        if (eval("/:Music_Movie") ne "Music_OnuKoro_2.swf") {
            Music_ChangeMusic = "1";
            Music_Movie = "Music_OnuKoro_2.swf";
        }
    } else {
        Music_ChangeMusic = "1";
        Music_Movie = "Music_OnuKoro.swf";
    }
    BeginLoadMovie = "1";
}
