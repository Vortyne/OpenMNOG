#version 4
if (eval("/:KoKoro") eq "7") {
    tellTarget ("Middle-Ground/Forward") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    loadMovie("sky4.swf", "/holder/Back-Ground/Sky-holder");
    loadMovie("Ihu.swf", "/holder/Back-Ground/Ihu-holder");
    Startedloading = "1";
}
