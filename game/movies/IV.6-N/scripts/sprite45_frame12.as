#version 4
RollOff = eval("../:RollOff");
Click = eval("../:Click");
if (Click eq "1") {
    tellTarget ("../Bow") {
        gotoAndPlay("play");
    }
    gotoAndStop(1);
} else if (RollOff eq "1") {
    tellTarget ("../TurnToBack") {
        gotoAndPlay("play");
    }
    tellTarget ("../") {
        RollOff = "0";
        Click = "0";
    }
    gotoAndStop(1);
} else {
    gotoAndPlay(11);
}
