#version 4
if (eval("/:Exit") eq "1") {
    gotoAndPlay(65);
} else {
    prevFrame();
    play();
}
