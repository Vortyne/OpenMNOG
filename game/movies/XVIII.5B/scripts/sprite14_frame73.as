#version 4
RandNum = random("8") + "1";
if (RandNum eq "1") {
    gotoAndPlay(1);
    play();
} else {
    gotoAndPlay(65);
    play();
}
