#version 4
tellTarget ("/") {
    Movieisloaded = "1";
}
if ("0" < eval("/:Launch9")) {
    CheckDrag = "1";
    setProperty("Rock1/Shadow", _visible, "0");
} else {
    startDrag("MouseDrag", "1");
}
tellTarget ("Rock1") {
    RockName = "Rock1";
}
TargetFrame = "91";
TargetDeviation = "10";
HoldTime = "4000";
