#version 4
XLoc = XLoc + eval("/:SlideX");
YLoc = YLoc + eval("/:SlideY");
setProperty("Target", _x, XLoc);
setProperty("Target", _y, YLoc);
