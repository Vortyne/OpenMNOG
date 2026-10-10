#version 4
RandNum = random("8") + "1";
if (RandNum eq "2") {
    gotoAndPlay(3);
} else if (RandNum eq "3") {
    gotoAndPlay(20);
}
