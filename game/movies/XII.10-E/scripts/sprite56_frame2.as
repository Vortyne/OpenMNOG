#version 4
xMouse = getProperty("/MouseDrag", _x) - xOffset;
yMouse = getProperty("/MouseDrag", _y) - yOffset;
yShadowScale = "100" + yMouse / "4";
if (yShadowScale < "1") {
    yShadowScale = "1";
}
if ("200" < yShadowScale) {
    yShadowScale = "100";
}
setProperty("Shadow", _yscale, yShadowScale);
setTarget("Shadow");
JumptoFrame = int("50" - eval("../:xMouse") / "5");
if ("99" < JumptoFrame) {
    JumptoFrame = "99";
} else if (JumptoFrame < "1") {
    JumptoFrame = "1";
}
gotoAndStop(JumptoFrame);
setTarget("");
