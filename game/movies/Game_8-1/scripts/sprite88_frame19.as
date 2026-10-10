#version 4
if (Hit eq "1") {
    gotoAndPlay(110);
} else if (Miss eq "1") {
    gotoAndPlay(81);
} else if (Target eq "") {
    if (eval("../:LastClick") eq name) {
        gotoAndPlay(12);
    } else {
        PrepAttack = "0";
        gotoAndPlay(4);
    }
} else {
    Attacking = "1";
    set("../:LastClick", "");
    AttackTime = eval("../:FramesPassed") + agility * "5";
    tellTarget ("Rahi") {
        Change = "1";
        gotoAndPlay("Ready");
    }
    gotoAndPlay(22);
}
