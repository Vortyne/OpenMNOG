#version 4
RandomAnimation = random("4") + "1";
if (RandomAnimation eq "1") {
    gotoAndPlay(7);
} else if (RandomAnimation eq "2") {
    gotoAndPlay(41);
} else if (RandomAnimation eq "3") {
    gotoAndPlay(79);
} else if (RandomAnimation eq "4") {
    gotoAndPlay(123);
}
