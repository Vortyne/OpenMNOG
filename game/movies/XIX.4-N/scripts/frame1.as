#version 4
tellTarget ("/") {
    loadMovie("sky3.swf", "/holder/Back-Ground/Sky-holder");
    loadMovie("Ihu.swf", "/holder/Back-Ground/Ihu-holder");
    Startedloading = "1";
}
if (eval("/:KoKoro") eq "1") {
    tellTarget ("Middle-Ground/Kopeke") {
        gotoAndStop(2);
    }
} else if (eval("/:KoKoro") >= "2") {
    tellTarget ("Middle-Ground/Door") {
        gotoAndStop("Open");
    }
}
