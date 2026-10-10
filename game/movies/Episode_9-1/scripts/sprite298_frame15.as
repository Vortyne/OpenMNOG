#version 4
if (random("4") + "1" eq "1") {
    nextFrame();
    play();
} else {
    gotoAndPlay(1);
}
