#version 4
X = "15";
RandX = random(X - "3") + "1";
RandNeg = random("2") + "1";
if (RandNeg eq "1") {
    RandX = RandX * "-1";
}
RandY = random(X + "3") + "1";
RandNeg = random("2") + "1";
if (RandNeg eq "1") {
    RandY = RandY * "-1";
}
setProperty("Hawk", _x, RandX);
setProperty("Hawk", _y, RandY);
