#version 4
RandNum = random("8") + "1";
if (RandNum eq "1") {
    gotoAndPlay(15);
} else {
    gotoAndPlay(1);
}
