#version 4
RandVar = random("3");
if (RandVar eq "1") {
    tellTarget ("../") {
        Farting = "1";
        Turning = "0";
        MoveShift = MoveShift * "-1";
        FartDist = FartDist * "-1";
    }
    setProperty("../Walking", _xscale, getProperty("../Walking", _xscale) * "-1");
    tellTarget ("../Farting") {
        gotoAndPlay("play");
    }
} else {
    tellTarget ("../") {
        Turning = "0";
        MoveCount = "0";
        MoveShift = MoveShift * "-1";
        FartDist = FartDist * "-1";
    }
    setProperty("../Walking", _xscale, getProperty("../Walking", _xscale) * "-1");
    tellTarget ("../Walking") {
        gotoAndPlay("play");
    }
}
