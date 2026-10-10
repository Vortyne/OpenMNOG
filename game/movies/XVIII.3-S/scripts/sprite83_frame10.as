#version 4
RandNum = random("12") + "1";
if (RandNum eq "1") {
    gotoAndPlay(1);
} else {
    gotoAndPlay(6);
}
