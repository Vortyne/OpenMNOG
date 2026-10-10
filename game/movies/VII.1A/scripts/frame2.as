#version 4
GaKoroVisited = eval("/:GaKoroVisited");
GaKoro = eval("/:GaKoro");
if (GaKoro eq "1" or GaKoroVisited eq "1") {
    loadMovie("MiniSub.swf", "/holder/TextSub");
} else {
    loadMovie("TextSub.swf", "/holder/TextSub");
}
tellTarget ("/") {
    Movieisloaded = "1";
    ShiftOff = "1";
}
