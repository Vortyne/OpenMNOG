#version 4
RandNum = random("15") + "1";
if (RandNum eq "1") {
    gotoAndPlay(6);
} else if (RandNum eq "2") {
    gotoAndPlay(11);
} else if (RandNum eq "3") {
    gotoAndPlay(16);
} else {
    gotoAndPlay(1);
    play();
}
