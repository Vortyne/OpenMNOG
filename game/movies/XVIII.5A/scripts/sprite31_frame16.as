#version 4
RandNum = random("16") + "1";
if (RandNum eq "1") {
    gotoAndPlay(17);
} else if (RandNum eq "2") {
    gotoAndPlay(34);
} else {
    gotoAndPlay(1);
}
