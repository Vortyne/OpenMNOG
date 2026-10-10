#version 4
if (eval("/:fGaKoro") eq "10") {
    tellTarget ("Middle-Ground/Fortifications1") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Fortifications2") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
