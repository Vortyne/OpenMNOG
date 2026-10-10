#version 4
RandNum = random("4") + "1";
if (RandNum eq "1") {
    gotoAndPlay(13);
} else {
    gotoAndPlay(1);
}
