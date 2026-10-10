#version 4
RandNum = random("12") + "1";
if (RandNum eq "1") {
    gotoAndPlay(12);
} else if (RandNum eq "2") {
    gotoAndPlay(26);
} else if (RandNum eq "3") {
    gotoAndPlay(38);
} else if (RandNum eq "4") {
    gotoAndPlay(65);
} else {
    gotoAndPlay(1);
}
