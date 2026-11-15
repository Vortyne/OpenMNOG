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
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:OnuKoroLava") eq "1") {
    tellTarget ("Door") {
        gotoAndStop(2);
    }
}
Language = eval("/:Language");
Gnomon = eval("/:Gnomon");
LetterA = eval("/:LetterA");
// (OpenMNOG: also once the gnomon is on the sundial, Gnomon 2; before, he went back to his first talk and gave
// another letter.)
if (Gnomon eq "1" or Gnomon eq "2") {
    TextToLoad = "Text_XII.9A_3_" add Language add ".txt";
} else if (LetterA eq "1") {
    TextToLoad = "Text_XII.9A_2_" add Language add ".txt";
} else {
    TextToLoad = "Text_XII.9A_1_" add Language add ".txt";
}
