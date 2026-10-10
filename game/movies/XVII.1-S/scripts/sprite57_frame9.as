#version 4
RandNum = random("6") + "1";
if (RandNum eq "1") {
    gotoAndPlay(10);
} else if (RandNum eq "2") {
    gotoAndPlay(34);
} else if (RandNum eq "3") {
    gotoAndPlay(56);
} else if (RandNum eq "4") {
    gotoAndPlay(76);
} else {
    gotoAndPlay(1);
}
