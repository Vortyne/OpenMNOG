#version 4
count = "0";
while (count < PlanksNum) {
    count = count + "1";
    LeftSide = getProperty("../LeftRope", _xscale) / "100" * getProperty("../LeftRope/Placer" add count, _x) + getProperty("../LeftRope", _x) + getProperty("../Planks", _x);
    RightSide = "0" - (getProperty("../RightRope", _xscale) / "100" * getProperty("../RightRope/Placer" add count, _x) - getProperty("../RightRope", _x) + getProperty("../Planks", _x));
    setProperty("Plank" add count, _x, (RightSide + LeftSide) / "2");
    setProperty("Plank" add count, _width, RightSide - LeftSide);
}
gotoAndPlay(2);
