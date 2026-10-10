#version 4
GaKoro = eval("/:GaKoro");
if (GaKoro eq "1") {
    gotoAndPlay(3);
}
tellTarget ("/") {
    Startedloading = "1";
}
tellTarget ("/holder/Middle-Ground/Light2/Lightfish") {
    gotoAndPlay(30);
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
