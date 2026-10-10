#version 4
RandNum = random("10") + "1";
if (RandNum eq "1") {
    gotoAndPlay(14);
} else if (RandNum eq "2") {
    gotoAndPlay(1);
} else {
    gotoAndPlay(2);
}
