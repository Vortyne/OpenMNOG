#version 4
RandNum = random("5") + "1";
if (RandNum eq "1") {
    gotoAndPlay(1);
} else {
    gotoAndPlay(11);
}
