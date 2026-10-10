#version 4
if (eval("/:Quest") >= "3") {
    tellTarget ("Middle-Ground/Forward") {
        gotoAndStop(2);
    }
    tellTarget ("DEBUG") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
    Startedloading = "1";
}
