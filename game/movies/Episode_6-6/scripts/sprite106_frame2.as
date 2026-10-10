#version 4
RandNum = random("7") + "1";
if (RandNum eq "1") {
    nextFrame();
    play();
} else {
    gotoAndPlay(1);
}
