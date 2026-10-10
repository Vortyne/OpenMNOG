#version 4
Count = "0";
tellTarget ("/") {
    Startedloading = "1";
}
Language = eval("/:Language");
IV6AVisited = eval("/:IV6AVisited");
GaKoro = eval("/:GaKoro");
if (eval("/:cKapura") eq "1") {
    TextToLoad = "Text_IV.6A_4_" add Language add ".txt";
} else if (GaKoro eq "1") {
    TextToLoad = "Text_IV.6A_2B_" add Language add ".txt";
} else if (IV6AVisited eq "1") {
    TextToLoad = "Text_IV.6A_2_" add Language add ".txt";
    tellTarget ("/") {
        IV6AVisited = "0";
    }
} else {
    TextToLoad = "Text_IV.6A_1_" add Language add ".txt";
    tellTarget ("/") {
        IV6AVisited = "1";
    }
}
