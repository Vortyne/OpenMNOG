#version 4
Talking = eval("/holder/:Talking");
if (Talking eq "1") {
    RandNum = random("3");
    if (RandNum eq "1") {
        gotoAndPlay(84);
    } else if (RandNum eq "2") {
        gotoAndPlay(111);
    } else {
        gotoAndPlay(139);
    }
} else {
    RandNum = random("16") + "1";
    if (RandNum eq "1") {
        gotoAndPlay(17);
    } else if (RandNum eq "2") {
        gotoAndPlay(52);
    } else {
        gotoAndPlay(1);
    }
}
