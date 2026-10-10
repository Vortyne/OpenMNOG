#version 4
RandNum = random("10") + "1";
if (RandNum eq "1") {
    gotoAndPlay(3);
} else {
    gotoAndPlay(1);
}
