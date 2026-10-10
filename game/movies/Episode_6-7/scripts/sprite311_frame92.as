#version 4
RandNum = random("5") + "1";
if (RandNum eq "1") {
    gotoAndPlay(1);
} else if (RandNum eq "2") {
    gotoAndPlay(68);
} else if (RandNum eq "3") {
    gotoAndPlay(43);
} else {
    gotoAndPlay(93);
}
