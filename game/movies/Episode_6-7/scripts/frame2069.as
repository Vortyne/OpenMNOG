#version 4
if (frameLoaded(492)) {
    set("/:Exit", "1");
}
if (eval("/:Exit") eq "1") {
    gotoAndPlay(2070);
} else {
    gotoAndPlay(2054);
}
