#version 4
RandNum = random("4") + "1";
if (RandNum eq "1") {
    gotoAndPlay(43);
} else if (RandNum eq "2") {
    gotoAndPlay(68);
} else {
    gotoAndPlay(7);
}
