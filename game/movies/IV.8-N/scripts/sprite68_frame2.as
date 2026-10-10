#version 4
setProperty("WalkingGuard", _x, getProperty("WalkingGuard", _x) + xmove);
setProperty("ClickOn", _x, getProperty("ClickOn", _x) + xmove);
WalkLoop = WalkLoop + "1";
