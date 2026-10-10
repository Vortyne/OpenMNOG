#version 4
RandNum = random("4") + "1";
if (RandNum eq "1") {
    gotoAndPlay(14);
} else if (RandNum eq "2") {
    gotoAndPlay(47);
} else {
    gotoAndPlay(1);
}
