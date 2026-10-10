#version 4
RandX = random(eval("/:Shake")) + "1" - "3";
RandNeg = random("2") + "1";
if (RandNeg eq "1") {
    RandX = RandX * "-1";
}
RandY = random(eval("/:Shake")) + "1";
RandNeg = random("2") + "1";
if (RandNeg eq "1") {
    RandY = RandY * "-1";
}
setProperty("King", _x, RandX);
setProperty("King", _y, RandY);
