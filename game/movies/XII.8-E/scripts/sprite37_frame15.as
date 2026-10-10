#version 4
RandNum = random("8") + "1";
if (RandNum eq "2") {
    gotoAndPlay(16);
} else {
    gotoAndPlay(1);
}
