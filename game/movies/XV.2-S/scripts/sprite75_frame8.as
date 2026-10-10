#version 4
RandNum = random("4") + "1";
if (RandNum eq "1") {
    gotoAndPlay(9);
} else {
    gotoAndPlay(1);
}
