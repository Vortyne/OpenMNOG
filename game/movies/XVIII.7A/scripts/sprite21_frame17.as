#version 4
Talking = eval("/holder/:Talking");
if (Talking eq "1") {
    RandNum = random("3");
    if (RandNum eq "1") {
        gotoAndPlay(79);
    } else if (RandNum eq "2") {
        gotoAndPlay(109);
    } else {
        gotoAndPlay(139);
    }
} else {
    RandNum = random("16") + "1";
    if (RandNum eq "1") {
        gotoAndPlay(18);
    } else if (RandNum eq "2") {
        gotoAndPlay(32);
    } else {
        gotoAndPlay(1);
    }
}
