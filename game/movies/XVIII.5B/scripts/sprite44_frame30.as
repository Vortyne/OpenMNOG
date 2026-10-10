#version 4
RandNum = random("20") + "1";
if (RandNum eq "1") {
    nextFrame();
    play();
} else if (RandNum eq "20") {
    gotoAndPlay(39);
} else {
    prevFrame();
    play();
}
