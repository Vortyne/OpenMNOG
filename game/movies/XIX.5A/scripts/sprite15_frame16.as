#version 4
RandNum = random("16") + "1";
if (RandNum eq "1") {
    gotoAndPlay(17);
} else if (RandNum eq "2") {
    gotoAndPlay(52);
} else {
    gotoAndPlay(1);
}
