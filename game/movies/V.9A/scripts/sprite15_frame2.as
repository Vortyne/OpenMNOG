#version 4
Talking = eval("/holder/:Talking");
if (Talking eq "1") {
    RandNum = random("2");
    if (RandNum eq "1") {
        gotoAndPlay(25);
    }
} else {
    gotoAndPlay(1);
}
