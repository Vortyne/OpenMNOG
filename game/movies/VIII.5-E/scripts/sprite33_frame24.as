#version 4
RandNum = random("6") + "1";
if (RandNum eq "1") {
    gotoAndPlay(25);
} else {
    gotoAndPlay(1);
}
