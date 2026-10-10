#version 4
RandNum = random("16") + "1";
if ("4" >= RandNum) {
    gotoAndPlay(13);
} else if (RandNum eq "5") {
    gotoAndPlay(38);
} else {
    gotoAndPlay(1);
}
