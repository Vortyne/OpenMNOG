#version 4
if (eval("/:KoKoro") eq "7") {
    tellTarget ("Middle-Ground/Avalanche") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    loadMovie("sky4.swf", "/holder/Back-Ground/Sky-holder");
    Startedloading = "1";
}
