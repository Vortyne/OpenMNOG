#version 4
Talking = eval("/holder/:Talking");
if (Talking != "1") {
    gotoAndPlay(1);
} else {
    RandNum = random("12") + "1";
    if (RandNum < "6") {
        gotoAndPlay(9);
    } else {
        gotoAndPlay(33);
    }
}
