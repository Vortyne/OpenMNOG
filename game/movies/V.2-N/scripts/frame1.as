#version 4
if (eval("/:Quest") >= "1") {
    tellTarget ("Middle-Ground/Forward") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    loadMovie("sky1.swf", "/holder/Back-ground/Sky-holder");
    Startedloading = "1";
}
