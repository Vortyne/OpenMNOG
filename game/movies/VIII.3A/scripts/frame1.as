#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
Pokoro = eval("/:Pokoro");
KohliBallUsed = eval("/:KohliBallUsed");
VIII3AVisited = eval("/:VIII3AVisited");
Chisel = eval("/:Chisel");
Book = eval("/:Book");
if (eval("/:cKapura") eq "1") {
    if (eval("/:cHafu") eq "1") {
        TextToLoad = "Text_VIII.3A_9_" add Language add ".txt";
    } else {
        TextToLoad = "Text_VIII.3A_8_" add Language add ".txt";
    }
} else if (Pokoro eq "1") {
    if (Chisel eq "1" or Book eq "1") {
        TextToLoad = "Text_VIII.3A_6_" add Language add ".txt";
    } else {
        TextToLoad = "Text_VIII.3A_5_" add Language add ".txt";
    }
} else if (KohliBallUsed eq "1") {
    TextToLoad = "Text_VIII.3A_4_" add Language add ".txt";
} else if (VIII3AVisited < "1") {
    VIII3AVisited = "1";
    TextToLoad = "Text_VIII.3A_1_" add Language add ".txt";
} else {
    TextToLoad = "Text_VIII.3A_3_" add Language add ".txt";
}
