#version 4
if (MoveCount < MoveLength) {
    setProperty("Walking", _x, getProperty("Walking", _x) + MoveShift);
    setProperty("Turning", _x, getProperty("Walking", _x) + MoveShift);
    setProperty("Farting", _x, getProperty("Walking", _x) + MoveShift);
    MoveCount = MoveCount + "1";
} else if (Turning eq "1") {
} else if (Farting eq "1") {
    if (Movefart eq "1") {
        setProperty("Walking", _x, getProperty("Walking", _x) + FartDist);
        setProperty("Turning", _x, getProperty("Walking", _x) + FartDist);
        setProperty("Farting", _x, getProperty("Walking", _x) + FartDist);
    }
} else {
    Turning = "1";
    tellTarget ("Walking") {
        gotoAndStop("Stop");
    }
    setProperty("Turning", _xscale, getProperty("Turning", _xscale) * "-1");
    setProperty("Farting", _xscale, getProperty("Farting", _xscale) * "-1");
    tellTarget ("Turning") {
        gotoAndPlay("play");
    }
}
gotoAndPlay(2);
