#version 4
RandNum = random("3") + "1";
if (RandNum eq "1") {
    gotoAndPlay(48);
} else {
    nextFrame();
    play();
}
