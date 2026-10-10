#version 4
RandNum = random("8") + "1";
if (RandNum eq "1") {
    gotoAndPlay(11);
} else if (RandNum eq "2") {
    gotoAndPlay(26);
} else {
    gotoAndPlay(1);
}
