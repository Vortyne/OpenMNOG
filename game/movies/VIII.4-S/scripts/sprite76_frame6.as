#version 4
RandNum = random("10") + "1";
if (RandNum eq "1") {
    gotoAndPlay(7);
} else {
    gotoAndPlay(5);
}
