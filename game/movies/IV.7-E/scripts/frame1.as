#version 4
TaBridge = eval("/:TaBridge");
if (TaBridge eq "1") {
    setProperty("/holder/Back-Ground/Steps", _x, "-50");
    setProperty("/holder/Back-Ground/Steps", _y, "-25");
}
tellTarget ("/") {
    Startedloading = "1";
}
