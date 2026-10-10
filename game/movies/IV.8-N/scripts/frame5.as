#version 4
// (Port: the cable's length and the chair's place follow the pan position directly, each frame, instead of being
// nudged by the change since the last frame (whose steps didn't match each way: it drifted, then snapped at the
// ends). The same values as the original at its start (385: 100) and its ends (40: 47, 730: 159.8).)
DebugMGH = eval("/:DebugMGH");
if (ChairX0 eq "") {
    ChairX0 = getProperty("Middle-Ground/Chair", _x);
}
M = DebugMGH;
if (M < "40") {
    M = "40";
} else if ("730" < M) {
    M = "730";
}
if (M < "385") {
    Scale = "47" + ("53" * (M - "40")) / "345";
} else {
    Scale = "100" + ("59.8" * (M - "385")) / "345";
}
setProperty("Middle-Ground/Rope1", _xscale, Scale);
setProperty("Middle-Ground/Rope2", _xscale, Scale);
setProperty("Middle-Ground/Chair", _x, ChairX0 - "0.15" * (DebugMGH - "385"));
MGPosition = DebugMGH;
