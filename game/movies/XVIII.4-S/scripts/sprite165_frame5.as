#version 4
RandNum = random("20") + "1";
if (RandNum eq "1") {
    gotoAndPlay(6);
} else {
    gotoAndPlay(1);
}
