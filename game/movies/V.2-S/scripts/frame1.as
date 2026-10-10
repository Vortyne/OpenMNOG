#version 4
if (eval("/:fGaKoro") eq "10") {
    gotoAndPlay(4);
} else {
    tellTarget ("/") {
        Startedloading = "1";
    }
    Rollover1Playing = "0";
    loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
    GaKoro_GateOpen = eval("/:GaKoro_GateOpen");
    if (GaKoro_GateOpen eq "1") {
        tellTarget ("Middle-Ground/Gate") {
            gotoAndPlay("GateOpen");
        }
    }
}
