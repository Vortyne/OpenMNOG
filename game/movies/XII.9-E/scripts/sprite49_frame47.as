#version 4
if (eval("/:OnuKoroDoor") eq "1") {
} else {
    tellTarget ("/") {
        MovieName = "XII.9A.swf";
        TurnBack = "1";
        ShiftOff = "1";
        BeginLoadMovie = "1";
    }
}
stop();
