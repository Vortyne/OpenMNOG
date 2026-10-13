#version 4
Lwidth = getProperty("LeftRope", _width);
Rwidth = getProperty("LeftRope", _width);
// (Port: the left rope's starting scale, for sizing it past zero below.)
Lscale = getProperty("LeftRope", _xscale);
// (OpenMNOG: the ropes set right away, as each frame after does (from the bridge's own place: what ../:xOffset
// holds, which isn't there yet; and the layers' own places, as the player's DebugFGH and DebugMGH aren't either),
// then the planks to them: the first frame drawn already has the bridge in place, which snapped there a few frames
// after the scene appeared.)
Lset = Lwidth - getProperty("/holder/Fore-Ground", _x) + getProperty("/holder/Middle-Ground", _x) - getProperty("../Bridge", _x);
Rset = Rwidth + getProperty("/holder/Fore-Ground", _x) - getProperty("/holder/Middle-Ground", _x) + getProperty("../Bridge", _x);
setProperty("LeftRope", _xscale, Lscale * Lset / Lwidth);
setProperty("RightRope", _width, Rset);
tellTarget ("Planks") {
    count = "0";
    while (count < "8") {
        count = count + "1";
        LeftSide = getProperty("../LeftRope", _xscale) / "100" * getProperty("../LeftRope/Placer" add count, _x) + getProperty("../LeftRope", _x) + getProperty("../Planks", _x);
        RightSide = "0" - (getProperty("../RightRope", _xscale) / "100" * getProperty("../RightRope/Placer" add count, _x) - getProperty("../RightRope", _x) + getProperty("../Planks", _x));
        setProperty("Plank" add count, _x, (RightSide + LeftSide) / "2");
        setProperty("Plank" add count, _width, RightSide - LeftSide);
    }
}
