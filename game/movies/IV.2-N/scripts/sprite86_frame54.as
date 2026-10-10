#version 4
RollOver = eval("../:RollOver");
if (RollOver eq "1") {
    tellTarget ("../TurnToward") {
        gotoAndPlay("play");
    }
    tellTarget ("../") {
        RollOver = "0";
    }
    gotoAndStop(55);
} else {
    gotoAndPlay(1);
}
