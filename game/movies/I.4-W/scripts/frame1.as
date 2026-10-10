#version 4
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
I4WVisited = eval("/:I4WVisited");
if (I4WVisited eq "1") {
    setProperty("/holder/Middle-Ground/Button-Tahu", _y, "2000");
} else {
    setProperty("/holder/Middle-Ground/Button_Forward", _y, "2000");
}
tellTarget ("/") {
    Startedloading = "1";
}
