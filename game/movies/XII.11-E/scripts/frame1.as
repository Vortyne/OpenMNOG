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
if (eval("/:Quest") >= "5") {
    tellTarget ("Mask") {
        gotoAndStop(2);
    }
    tellTarget ("Nub") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
