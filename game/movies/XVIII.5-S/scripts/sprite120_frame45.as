#version 4
RandNum = random("8") + "1";
if (RandNum eq "1") {
    gotoAndPlay(30);
} else if (RandNum eq "2") {
    gotoAndPlay(46);
} else {
    gotoAndPlay(41);
}
