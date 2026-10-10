#version 4
Talking = eval("/holder/:Talking");
if (Talking eq "1") {
    RandNum = random("3");
    if (RandNum eq "1") {
        gotoAndPlay(39);
    } else if (RandNum eq "2") {
        gotoAndPlay(66);
    } else {
        gotoAndPlay(94);
    }
} else {
    RandNum = random("8") + "1";
    if (RandNum eq "1") {
        gotoAndPlay(17);
    } else {
        gotoAndPlay(1);
    }
}
