#version 5
RanScale = random(40) + 80;
setProperty("FireWork", _xscale, RanScale);
setProperty("FireWork", _yscale, RanScale);
RanPosX = random(200) + 285;
RanPosY = random(200) + 112;
setProperty("FireWork", _x, RanPosX);
setProperty("FireWork", _y, RanPosY);
tellTarget ("FireWork") {
    gotoAndPlay("Play");
}
