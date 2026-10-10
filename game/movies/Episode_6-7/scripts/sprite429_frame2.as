#version 4
XLoc = XLoc + eval("/:SlideX2");
YLoc = YLoc + eval("/:SlideY2");
setProperty("Target", _x, XLoc);
setProperty("Target", _y, YLoc);
