#version 4
RandNum = random("16") + "1";
if ("4" >= RandNum) {
    gotoAndPlay(7);
} else if (RandNum eq "5") {
    gotoAndPlay(18);
} else {
    gotoAndPlay(1);
}
