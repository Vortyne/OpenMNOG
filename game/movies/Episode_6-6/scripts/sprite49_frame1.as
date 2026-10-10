#version 4
X = "5";
RandX = random(X - "2") + "1";
RandNeg = random("2") + "1";
if (RandNeg eq "1") {
    RandX = RandX * "-1";
}
RandY = random(X + "2") + "1";
RandNeg = random("2") + "1";
if (RandNeg eq "1") {
    RandY = RandY * "-1";
}
setProperty("Wing", _x, RandX);
setProperty("Wing", _y, RandY);
