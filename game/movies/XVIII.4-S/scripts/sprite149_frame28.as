#version 4
RandNum = random("6") + "1";
if (RandNum eq "1") {
    gotoAndPlay(1);
} else if (RandNum eq "2") {
    gotoAndPlay(29);
} else {
    gotoAndPlay(20);
}
