#version 4
// (OpenMNOG: this part of the mine has no music, as before; Onu-Koro's second music, played in its other scenes
// once the lava is fixed, stops here.)
if (eval("/:Music_Movie") eq "Music_OnuKoro_2.swf") {
    tellTarget ("/") {
        Music_Movie = "blank.swf";
        Music_ChangeMusic = "0";
    }
    loadMovie("blank.swf", "/Music-holder");
}
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:OnuKoroRats") eq "1") {
    gotoAndPlay(4);
}
