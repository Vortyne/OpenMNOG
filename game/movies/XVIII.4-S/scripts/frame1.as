#version 4
if (eval("/:Flute") eq "1") {
    setProperty("Middle-Ground/Flute", _y, "-2000");
}
tellTarget ("Middle-Ground/T1") {
    gotoAndStop(eval("/:LeKoro"));
}
tellTarget ("Middle-Ground/T2") {
    gotoAndStop(eval("/:LeKoro"));
}
tellTarget ("Middle-Ground/T3") {
    gotoAndStop(eval("/:LeKoro"));
}
if (eval("/:LeKoro") eq "5") {
    gotoAndPlay(4);
}
tellTarget ("/") {
    Startedloading = "1";
}
