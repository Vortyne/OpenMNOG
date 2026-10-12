#version 4
// (OpenMNOG: once the lava is fixed, Onu-Koro's second music plays here, carrying on from the last scene if it was
// already playing; before that, never: Onu-Koro's first music.)
if (eval("/:OnuKoroLava") eq "1") {
    if (eval("/:Music_Movie") ne "Music_OnuKoro_2.swf") {
        tellTarget ("/") {
            Music_Movie = "Music_OnuKoro_2.swf";
            Music_ChangeMusic = "0";
        }
        loadMovie("Music_OnuKoro_2.swf", "/Music-holder");
    }
} else if (eval("/:Music_Movie") eq "Music_OnuKoro_2.swf") {
    tellTarget ("/") {
        Music_Movie = "Music_OnuKoro.swf";
        Music_ChangeMusic = "0";
    }
    loadMovie("Music_OnuKoro.swf", "/Music-holder");
}
if (eval("/:OnuKoroLava") eq "1") {
    gotoAndPlay(17);
}
tellTarget ("/") {
    Startedloading = "1";
}
