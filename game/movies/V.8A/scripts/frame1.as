#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
V8AVisited = eval("/:V8AVisited");
GaKoro = eval("/:GaKoro");
PoKoro = eval("/:PoKoro");
Book = eval("/:Book");
if (eval("/:cKapura") eq "1") {
    if (eval("/:cMaku") eq "1") {
        TextToLoad = "Text_V.8A_7_" add Language add ".txt";
    } else {
        TextToLoad = "Text_V.8A_6_" add Language add ".txt";
    }
} else if (V8AVisited eq "1" or Pokoro eq "1" or Book eq "1") {
    TextToLoad = "Text_V.8A_2_" add Language add ".txt";
} else {
    TextToLoad = "Text_V.8A_1_" add Language add ".txt";
    V8AVisited = "1";
}
