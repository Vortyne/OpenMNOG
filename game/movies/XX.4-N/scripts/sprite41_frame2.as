#version 4
RandNum = random("3") + "1";
if (RandNum eq "1") {
    gotoAndPlay(1);
} else if (RandNum eq "2") {
    gotoAndPlay(3);
} else if (RandNum eq "3") {
    gotoAndPlay(5);
}
