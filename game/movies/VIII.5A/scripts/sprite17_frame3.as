#version 4
Talking = eval("/holder/:Talking");
if (Talking eq "1") {
    RandNum = random("3") + "1";
    if (RandNum eq "1") {
        gotoAndPlay(4);
    } else if (RandNum eq "2") {
        gotoAndPlay(45);
    } else if (RandNum eq "3") {
        gotoAndPlay(102);
    }
} else {
    gotoAndPlay(2);
}
