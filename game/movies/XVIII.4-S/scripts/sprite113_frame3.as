#version 4
RandNum = random("7") + "1";
if (RandNum eq "1") {
    gotoAndPlay(3);
} else if (RandNum eq "2") {
    gotoAndPlay(6);
} else {
    gotoAndPlay(1);
}
