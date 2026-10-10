#version 4
WalkClick = eval("../:WalkClick");
ReturnTo = "1";
if (WalkClick eq "1") {
    setTarget("../");
    AllowTurn = "1";
    if (TurnRight eq "1") {
        setProperty("TurnStop", _x, getProperty("WalkingGuard", _x) - "16.1");
        setProperty("TurnStop", _y, getProperty("WalkingGuard", _y) + "11.5");
    } else {
        setProperty("TurnStop", _x, getProperty("WalkingGuard", _x) + "16.1");
        setProperty("TurnStop", _y, getProperty("WalkingGuard", _y) + "11.5");
    }
    gotoAndPlay("TurnandStop");
    setTarget("");
    gotoAndStop(18);
}
