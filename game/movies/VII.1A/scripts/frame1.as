#version 4
loadMovie("sky2.swf", "/holder/Back-Ground/Sky-holder");
tellTarget ("/") {
    Startedloading = "1";
    SailTo = "GaKoro";
    MakuTells = "PoWahi";
}
Count = "0";
Language = eval("/:Language");
Episode3_2Visited = eval("/:Episode3_2Visited");
I6Visited = eval("/:I6Visited");
GaKoroVisited = eval("/:GaKoroVisited");
GaKoro = eval("/:GaKoro");
if (GaKoro eq "1" or GaKoroVisited eq "1") {
    TextToLoad = "Text_VII.1A_4_" add Language add ".txt";
    setProperty("Middle-Ground/Maku", _x, "3000");
} else if (Episode3_2Visited eq "1") {
    tellTarget ("/") {
        Episode3_2Visited = "0";
    }
    TextToLoad = "Text_VII.1A_2_" add Language add ".txt";
} else if (I6Visited eq "1") {
    TextToLoad = "Text_VII.1A_3_" add Language add ".txt";
} else {
    TextToLoad = "Text_VII.1A_1_" add Language add ".txt";
    tellTarget ("/") {
        I6Visited = "1";
    }
}
