#version 4
if (eval("/:Pokoro") eq "1") {
    setProperty("/holder/Middle-Ground/X", _y, "2000");
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
tellTarget ("/") {
    Startedloading = "1";
}
