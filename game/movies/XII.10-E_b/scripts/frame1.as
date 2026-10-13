#version 4
// (OpenMNOG: once the lava is fixed, Onu-Koro's second music plays here, carrying on from the last scene if it was
// already playing; before that, never: no music, as before.)
if (eval("/:OnuKoroLava") eq "1") {
    if (getProperty("/Music-holder", _url) ne "Music_OnuKoro_2.swf") {
        tellTarget ("/") {
            Music_Movie = "Music_OnuKoro_2.swf";
            Music_ChangeMusic = "0";
        }
        loadMovie("Music_OnuKoro_2.swf", "/Music-holder");
    }
} else if (getProperty("/Music-holder", _url) eq "Music_OnuKoro_2.swf") {
    tellTarget ("/") {
        Music_Movie = "blank.swf";
        Music_ChangeMusic = "0";
    }
    loadMovie("blank.swf", "/Music-holder");
}
if (eval("/:OnuKoroDoor") eq "1") {
    gotoAndPlay(223);
}
tellTarget ("/") {
    Startedloading = "1";
}
