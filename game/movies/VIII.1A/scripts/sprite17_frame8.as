#version 4
Talking = eval("/holder/:Talking");
if (Talking != "1") {
    RandNum = random("12") + "1";
    if (RandNum eq "1") {
        gotoAndPlay(9);
    } else {
        gotoAndPlay(1);
    }
} else {
    RandNum = random("12") + "1";
    if (RandNum < "6") {
        gotoAndPlay(29);
    } else {
        gotoAndPlay(58);
    }
}
