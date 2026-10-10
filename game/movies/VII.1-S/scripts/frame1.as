#version 4
tellTarget ("/") {
    Startedloading = "1";
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
if (eval("/:GakoroVisited") eq "1" or eval("/:Gakoro") eq "1") {
    setProperty("Middle-Ground/Maku", _visible, "0");
}
