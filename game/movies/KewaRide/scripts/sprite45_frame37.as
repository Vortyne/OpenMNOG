#version 4
RandNum = random("6") + "1";
if (RandNum eq "1") {
    gotoAndPlay(38);
} else if (RandNum eq "2") {
    gotoAndPlay(54);
} else if (RandNum eq "3") {
    gotoAndPlay(70);
} else {
    gotoAndPlay(32);
}
