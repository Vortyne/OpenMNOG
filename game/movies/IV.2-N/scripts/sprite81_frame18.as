#version 4
RollOff = eval("../:RollOff");
if (RollOff eq "1") {
    tellTarget ("../TurnBack") {
        gotoAndPlay("play");
    }
    tellTarget ("../") {
        RollOff = "0";
    }
    gotoAndStop(1);
} else {
    gotoAndPlay(17);
}
