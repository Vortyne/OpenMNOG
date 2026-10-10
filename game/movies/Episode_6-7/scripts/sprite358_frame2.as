#version 4
XLoc = XLoc + eval("/:SlideX");
YLoc = YLoc + eval("/:SlideY");
setProperty("King", _x, XLoc);
setProperty("King", _y, YLoc);
