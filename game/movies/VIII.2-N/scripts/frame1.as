#version 4
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:PoKoro") eq "1") {
    gotoAndPlay(4);
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
