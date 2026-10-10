#version 4
RandNum = random("24") + "1";
if (RandNum eq "1") {
    gotoAndPlay(7);
} else if (RandNum eq "2") {
    gotoAndPlay(18);
} else if (RandNum eq "3") {
    gotoAndPlay(25);
} else if (RandNum eq "4") {
    gotoAndPlay(42);
} else {
    gotoAndPlay(1);
}
