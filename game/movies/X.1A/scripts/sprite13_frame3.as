#version 4
Talking = eval("/holder/:Talking");
if (Talking eq "1") {
    gotoAndPlay(4);
} else {
    gotoAndPlay(1);
}
