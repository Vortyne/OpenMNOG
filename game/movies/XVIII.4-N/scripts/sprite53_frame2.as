#version 4
xOffset = getProperty("LeftRope", _x);
// (OpenMNOG: from the layers' own places, which the player's DebugFGH and DebugMGH copy only from its next round,
// a frame late and not yet at all as the scene appears.)
Lset = Lwidth - getProperty("/holder/Fore-Ground", _x) + getProperty("/holder/Middle-Ground", _x) - eval("../:xOffset");
Rset = Rwidth + getProperty("/holder/Fore-Ground", _x) - getProperty("/holder/Middle-Ground", _x) + eval("../:xOffset");
// (Port: the ropes follow the panning all the way, not only while the left one is wider than 80 (it stopped there,
// left bowed out from the planks near the screen's right edge). Past zero the left one turns the other way, as it
// would in perspective (scaled, since a width can't be negative), instead of the bridge's near end passing it.)
setProperty("LeftRope", _xscale, Lscale * Lset / Lwidth);
setProperty("RightRope", _width, Rset);
