#version 4
RandNum = random("12") + "1";
if (RandNum eq "1") {
    gotoAndPlay(18);
} else if (RandNum eq "2") {
    gotoAndPlay(42);
} else {
    gotoAndPlay(1);
}
