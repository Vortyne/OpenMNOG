#version 4
RandNum = random("30") + "1";
if (RandNum eq "1") {
    gotoAndPlay(10);
} else if (RandNum eq "2") {
    gotoAndPlay(20);
} else {
    gotoAndPlay(1);
}
