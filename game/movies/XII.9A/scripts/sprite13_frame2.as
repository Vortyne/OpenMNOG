#version 4
Talking = eval("/holder/:Talking");
if (Talking eq "1") {
    RandNum = random("6") + "1";
    if (RandNum eq "1") {
        gotoAndPlay(34);
    }
} else {
    gotoAndPlay(1);
}
