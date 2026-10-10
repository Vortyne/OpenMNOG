#version 4
Talking = eval("/holder/:Talking");
if (Talking eq "0") {
    gotoAndPlay(1);
} else {
    gotoAndPlay(3);
}
