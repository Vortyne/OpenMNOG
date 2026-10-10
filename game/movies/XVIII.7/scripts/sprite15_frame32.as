#version 4
RandNum = random("42") + "1";
if (RandNum eq "1") {
    gotoAndPlay(1);
} else {
    prevFrame();
    play();
}
