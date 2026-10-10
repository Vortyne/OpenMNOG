#version 4
RandNum = random("8") + "1";
if (RandNum eq "1") {
    gotoAndPlay(8);
} else if (RandNum eq "2") {
    gotoAndPlay(27);
} else {
    gotoAndPlay(1);
}
