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
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
if (eval("/:TaipuCanGo") eq "1") {
    TextToLoad = "Text_XII9B_Taipu_" add Language add ".txt";
} else {
    TextToLoad = "Text_XII9B_Taipu_2_" add Language add ".txt";
}
