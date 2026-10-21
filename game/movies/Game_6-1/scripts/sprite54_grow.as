#version 4
// OpenMNOG: the clouds grow a little every frame of the loop (frames 3 to 16, the ones shown: this frame's share of the next step,
// which the loop's last frame, 17, adds to NestScale and sets), at the same speed as before but not at once.
Grow = "100" - eval("../../:NestTransition2") + eval("../../:NestScale") + (getProperty("", _currentframe) - "3") * eval("../../:NestJump") / "14";
setProperty("GrowClouds", _xscale, Grow);
setProperty("GrowClouds", _yscale, Grow);
