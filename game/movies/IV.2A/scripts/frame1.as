#version 4
loadMovie("sky2.swf", "/holder/Back-Ground/Sky-holder", "GET");
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
GaKoro = eval("/:GaKoro");
if (GaKoro eq "1" and eval("/:LeKoro") eq "5" and eval("/:PoKoro") eq "1" and eval("/:OnuKoroDoor") eq "1" and eval("/:OnuKoroLava") eq "1") {
    if (eval("/:Ensign") eq "1") {
        if (eval("/:KoKoro") >= "2") {
            TextToLoad = "Text_IV.2A_5_" add Language add ".txt";
        } else {
            TextToLoad = "Text_IV.2A_4_" add Language add ".txt";
        }
    } else {
        TextToLoad = "Text_IV.2A_3_" add Language add ".txt";
    }
    JalaState = "3";
} else if (GaKoro eq "1") {
    TextToLoad = "Text_IV.2A_2_" add Language add ".txt";
} else {
    TextToLoad = "Text_IV.2A_" add Language add ".txt";
}
