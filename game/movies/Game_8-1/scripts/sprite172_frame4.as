#version 4
if (eval("../:LastClick") eq "") {
    gotoAndPlay(1);
} else {
    gotoAndPlay(3);
}
