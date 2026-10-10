#version 4
LBodyXLoc = LBodyXLoc + eval("/:LBodySlideX");
LBodyYLoc = LBodyYLoc + eval("/:LBodySlideY");
setProperty("Target", _x, LBodyXLoc);
setProperty("Target", _y, LBodyYLoc);
if ("1" < eval("/:LBodyShake")) {
    RandX = random(eval("/:LBodyShake")) + "1";
    RandNeg = random("2") + "1";
    if (RandNeg eq "1") {
        RandX = RandX * "-1";
    }
    RandY = random(eval("/:LBodyShake")) + "1";
    RandNeg = random("2") + "1";
    if (RandNeg eq "1") {
        RandY = RandY * "-1";
    }
    setProperty("Target", _x, RandX);
    setProperty("Target", _y, RandY);
}
