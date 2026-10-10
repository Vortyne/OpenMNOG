#version 4
Defend = "1";
if (Hit eq "1") {
    gotoAndPlay(110);
} else if (Miss eq "1") {
    gotoAndPlay(81);
} else if (PrepAttack eq "1") {
    gotoAndPlay(12);
} else {
    gotoAndPlay(4);
}
