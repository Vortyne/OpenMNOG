#version 4
xPohatu = eval("../:xPohatu");
yPohatu = eval("../:yPohatu") - "40";
xRatio = (xPohatu - xMouse) / (yPohatu - yMouse);
yRatio = yPohatu - yMouse;
if (yRatio < "0") {
    xRatio = xRatio * "-1";
}
if ("0" < xRatio and "0.66" < xRatio) {
    xRatio = "0.66";
}
if (xRatio < "0" and xRatio < "-0.66") {
    xRatio = "-0.66";
}
yRatio = "1";
if ("-10" < Velocity) {
    Velocity = "-10";
}
if (Velocity < "-30") {
    Velocity = "-30";
}
ScaleCounter = Velocity / "-2";
xVelocity = xRatio * Velocity;
yVelocity = yRatio * Velocity;
ScaleMin = "100";
ScaleMax = "100" + yVelocity * "-2";
CollObjs = eval("../:CollObjs");
LoftCount = "1";
Roll = "0";
RollShift = "-0.04";
Collisions = "1";
Backward = "0";
Break = "0";
DeleteBall = "0";
ScaleDistanceShift = "0";
ActualScale = "100";
Lofting = "0";
trace(ScaleMax);
