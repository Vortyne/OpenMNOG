#version 4
Talking = eval("/holder/:Talking");
if (Talking eq "1") {
    RandVar = random("10");
    if (RandVar eq "7") {
        gotoAndPlay(3);
    } else {
        gotoAndPlay(1);
    }
} else {
    gotoAndPlay(1);
}
