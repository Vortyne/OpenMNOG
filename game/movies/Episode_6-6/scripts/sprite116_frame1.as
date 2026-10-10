#version 4
RandX = random("30") + "1";
RandNeg = random("2") + "1";
if (RandNeg eq "1") {
    RandX = RandX * "-1";
}
RandY = random("30") + "1";
RandNeg = random("2") + "1";
if (RandNeg eq "1") {
    RandY = RandY * "-1";
}
setProperty("Kongu", _x, RandX);
setProperty("Kongu", _y, "440" + RandY);
