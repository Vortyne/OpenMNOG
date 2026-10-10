#version 4
RandNum = random("12") + "1";
if (RandNum eq "1") {
    gotoAndPlay(7);
} else if (RandNum eq "2") {
    gotoAndPlay(24);
} else if (RandNum eq "3") {
    gotoAndPlay(30);
} else if (RandNum eq "4") {
    gotoAndPlay(37);
} else if (RandNum eq "5") {
    gotoAndPlay(68);
} else if (RandNum eq "6") {
    gotoAndPlay(80);
} else if (RandNum eq "7") {
    gotoAndPlay(93);
} else {
    gotoAndPlay(1);
}
