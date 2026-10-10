#version 4
if (eval("/:fGaKoro") eq "10") {
    tellTarget ("Middle-Ground/Fortifications") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
Rollover1Playing = "0";
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
