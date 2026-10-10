#version 4
RandNum = random("3") + "1";
if (RandNum eq "1") {
    gotoAndPlay(10);
} else {
    gotoAndPlay(1);
}
