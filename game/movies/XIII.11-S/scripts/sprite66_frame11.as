#version 4
RandNum = random("5") + "1";
if (RandNum eq "1") {
    gotoAndPlay(12);
} else {
    gotoAndPlay(1);
}
