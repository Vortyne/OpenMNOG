#version 4
RollOver = eval("../:RollOver");
if (Rollover eq "1") {
    tellTarget ("../TurnToFront") {
        gotoAndPlay("Play");
    }
    tellTarget ("../") {
        RollOver = "0";
    }
    gotoAndStop(10);
} else {
    gotoAndPlay(1);
}
