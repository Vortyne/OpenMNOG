#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
if (eval("/:cKapura") eq "1") {
    if (eval("/:cKopeke") eq "1") {
        TextToLoad = "Text_XX.5B_6_" add Language add ".txt";
    } else {
        TextToLoad = "Text_XX.5B_5_" add Language add ".txt";
    }
} else if (eval("/:LetterB") eq "1") {
    TextToLoad = "Text_XX.5B_4_" add Language add ".txt";
} else {
    TextToLoad = "Text_XX.5B_1_" add Language add ".txt";
}
