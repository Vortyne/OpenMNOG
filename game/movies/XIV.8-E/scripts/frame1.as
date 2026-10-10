#version 4
loadMovie("sky2.swf", "/holder/Back-Ground/Sky-holder");
I4WVisited = eval("/:I4WVisited");
if (I4WVisited eq "1") {
    setProperty("/holder/Middle-Ground/TahuButton", _y, "2000");
}
tellTarget ("/") {
    Startedloading = "1";
}
