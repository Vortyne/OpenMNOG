#version 4
// OpenMNOG: the nest and the hill grow a little every frame of the loop (frames 3 to 16, the ones shown: this frame's share of the
// next step, which the loop adds to NestScale on its last frame), at the same speed as before but not 1% at once
// every loop. On frames 1 and 2 too (before the loop, coming back from the clouds), so they're not shown first at
// their placed, far-off size.
Grow = eval("../../:NestScale") + (getProperty("", _currentframe) - "3") * eval("../../:NestJump") / "14";
setProperty("Nest", _xscale, Grow);
setProperty("Nest", _yscale, Grow);
setProperty("Hill", _xscale, Grow + "30");
setProperty("Hill", _yscale, Grow + "30");
