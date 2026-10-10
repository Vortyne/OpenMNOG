#version 4
randomvar = random("5");
if (randomvar eq "1") {
    gotoAndPlay(6);
} else if (randomvar eq "2") {
    gotoAndPlay(11);
} else if (randomvar eq "3") {
    gotoAndPlay(16);
} else if (randomvar eq "4") {
    gotoAndPlay(21);
} else {
    gotoAndPlay(26);
}
