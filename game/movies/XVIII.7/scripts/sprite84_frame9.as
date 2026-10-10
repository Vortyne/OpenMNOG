#version 4
RandNum = random("32") + "1";
if (RandNum eq "1") {
    gotoAndPlay(10);
} else if (RandNum eq "2") {
    gotoAndPlay(24);
} else if (RandNum eq "3") {
    gotoAndPlay(35);
    set("/:TipToe", "1");
} else {
    gotoAndPlay(1);
}
