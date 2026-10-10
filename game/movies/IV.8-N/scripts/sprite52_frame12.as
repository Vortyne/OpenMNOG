#version 4
TurnClick = eval("../:TurnClick");
if (eval("../../../:EnsignPass") eq "1") {
    setProperty("../../MntForward", _x, eval("../../:xMntForward"));
    setProperty("../../MntForward", _y, eval("../../:yMntForward"));
}
if (TurnClick eq "1") {
    gotoAndPlay(13);
} else {
    gotoAndPlay(11);
}
