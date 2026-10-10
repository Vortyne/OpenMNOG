#version 4
RandNum = random("5") + "1";
if (RandNum eq "1") {
    gotoAndPlay(68);
} else if (RandNum eq "2") {
    gotoAndPlay(1);
} else {
    gotoAndPlay(86);
}
