#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
GaKoro = eval("/:GaKoro");
V7AVisited = eval("/:V7AVisited");
if (V7AVisited eq "1") {
    TextToLoad = "Text_V.7A_2_" add Language add ".txt";
} else {
    TextToLoad = "Text_V.7A_1_" add Language add ".txt";
    tellTarget ("/") {
        V7AVisited = "1";
    }
}
