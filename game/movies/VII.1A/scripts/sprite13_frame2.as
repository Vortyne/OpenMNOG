#version 4
Talking = eval("/holder/:Talking");
if (Talking eq "0") {
    gotoAndPlay(1);
} else {
    RandVar = random("5");
    if (RandVar eq "3") {
        gotoAndPlay(51);
    } else {
        gotoAndPlay(3);
    }
}
