#version 4
RandNum = random("8") + "1";
if (RandNum eq "1") {
    gotoAndPlay(13);
} else {
    gotoAndPlay(3);
}
