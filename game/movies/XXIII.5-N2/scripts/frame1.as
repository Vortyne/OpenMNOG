#version 4
if (eval("/:Quest") >= "5") {
    tellTarget ("Middle-Ground/Toa") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
    Startedloading = "1";
}
