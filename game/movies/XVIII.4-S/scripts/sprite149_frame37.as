#version 4
RandNum = random("6") + "1";
if (RandNum eq "1") {
    gotoAndPlay(10);
} else if (RandNum eq "2") {
    gotoAndPlay(38);
} else {
    gotoAndPlay(1);
}
