#version 4
// (OpenMNOG: once the lava is fixed, Onu-Koro's second music plays here, carrying on from the last scene if it was
// already playing; before that, never: no music, as before.)
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
        Music_Movie = "blank.swf";
        Music_ChangeMusic = "0";
    }
    loadMovie("blank.swf", "/Music-holder");
}
if (eval("/:Quest") < "6") {
    if (eval("/:OnuKoroLava") eq "1") {
        gotoAndPlay(4);
    }
}
if (eval("/:Quest") >= "6") {
    tellTarget ("Middle-Ground/Tohunga") {
        gotoAndStop(2);
    }
}
if (eval("/:OnuKoroDoor") eq "1") {
    tellTarget ("/holder/Middle-Ground/Door") {
        gotoAndStop(3);
    }
}
TurnBack = eval("/:TurnBack");
if (TurnBack eq "1") {
    tellTarget ("/holder/Middle-Ground/Prospector") {
        gotoAndPlay("TurnBack");
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
