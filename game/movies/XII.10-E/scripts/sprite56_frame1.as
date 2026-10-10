#version 4
RockName = "Rock1";
xOffset = getProperty("../" add RockName, _x);
yOffset = getProperty("../" add RockName, _y);
xShadow = getProperty("Shadow", _x);
yShadow = getProperty("Shadow", _y);
xShadowSize = getProperty("Shadow", _width);
yShadowSize = getProperty("Shadow", _height);
tellTarget ("Shadow") {
    gotoAndStop(50);
}
