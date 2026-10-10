#version 4
if (WalkLoop < WalkCount) {
    gotoAndPlay(2);
} else {
    WalkLoop = "1";
    if (TurnRight eq "1") {
        TurnRight = "0";
        tellTarget ("WalkingGuard") {
            gotoAndStop("StopWalking");
        }
        setProperty("WalkingGuard", _xscale, getProperty("WalkingGuard", _xscale) * "-1");
        setProperty("TurnStop", _xscale, getProperty("TurnStop", _xscale) * "-1");
        gotoAndPlay(7);
    } else if (TurnRight eq "0") {
        TurnRight = "1";
        tellTarget ("WalkingGuard") {
            gotoAndStop("StopWalking");
        }
        setProperty("WalkingGuard", _xscale, getProperty("WalkingGuard", _xscale) * "-1");
        setProperty("TurnStop", _xscale, getProperty("TurnStop", _xscale) * "-1");
        gotoAndPlay(14);
    }
}
