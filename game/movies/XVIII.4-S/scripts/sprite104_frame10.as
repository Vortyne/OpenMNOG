#version 4
RandNum = random("8") + "1";
if (RandNum eq "1") {
    gotoAndPlay(1);
} else if (RandNum eq "2") {
    gotoAndPlay(11);
} else if (RandNum eq "3") {
    gotoAndPlay(16);
} else {
    gotoAndPlay(6);
    play();
}
