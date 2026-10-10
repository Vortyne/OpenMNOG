#version 4
RandNum = random("12") + "1";
if (RandNum eq "1") {
    gotoAndPlay(24);
} else {
    gotoAndPlay(1);
    play();
}
