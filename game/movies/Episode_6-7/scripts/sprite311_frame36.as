#version 4
RandNum = random("6") + "1";
if (RandNum eq "1") {
    gotoAndPlay(7);
} else if (RandNum eq "2") {
    gotoAndPlay(68);
} else {
    gotoAndPlay(37);
}
