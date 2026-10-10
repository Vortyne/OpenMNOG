#version 4
if (Alive eq "1") {
    Defend = "1";
    RandAttack = random("20");
    if (RandAttack eq "13") {
        if (Attacking eq "0" and PrepAttack eq "0") {
            PrepAttack = "1";
        }
    }
    if (Hit eq "1") {
        gotoAndPlay(107);
    } else if (Miss eq "1") {
        gotoAndPlay(91);
    } else if (PrepAttack eq "1") {
        gotoAndPlay(13);
    } else {
        gotoAndPlay(4);
    }
} else {
    gotoAndStop(1);
}
