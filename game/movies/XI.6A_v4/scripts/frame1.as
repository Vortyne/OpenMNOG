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
tellTarget ("/") {
    Startedloading = "1";
}
Language = eval("/:Language");
if (eval("/:Quest") < "6") {
    if (eval("/:cKapura") eq "1") {
        set("/:TaipuCanGo", "1");
        if (eval("/:cTaipu") eq "1") {
            TextToLoad = "Text_XI.6A_5_" add Language add ".txt";
        } else {
            if (eval("/:Taipu") eq "cBar") {
                Taipu = eval("/:TaipuWas");
            } else {
                Taipu = eval("/:Taipu");
            }
            if (Taipu eq "Free") {
                TextToLoad = "Text_XI.6A_3_" add Language add ".txt";
            } else {
                TextToLoad = "Text_XI.6A_4_" add Language add ".txt";
            }
            tellTarget ("/") {
                TaipuWas = Taipu;
                Taipu = "cBar";
            }
        }
    } else {
        TextToLoad = "Text_XI.6A_1_" add Language add ".txt";
    }
} else {
    TextToLoad = "Text_XI.6A_6_" add Language add ".txt";
}
