#version 4
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
