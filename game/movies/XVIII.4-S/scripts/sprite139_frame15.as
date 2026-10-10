#version 4
RandNum = random("6") + "1";
if (RandNum eq "1") {
    gotoAndPlay(16);
} else if (RandNum eq "2") {
    gotoAndPlay(6);
} else if (RandNum eq "3") {
    gotoAndPlay(1);
} else {
    gotoAndPlay(11);
}
