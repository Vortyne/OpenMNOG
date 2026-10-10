#version 4
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
if (Gnomon eq "1") {
    TextToLoad = "Text_XII.9A_3_" add Language add ".txt";
} else if (LetterA eq "1") {
    TextToLoad = "Text_XII.9A_2_" add Language add ".txt";
} else {
    TextToLoad = "Text_XII.9A_1_" add Language add ".txt";
}
