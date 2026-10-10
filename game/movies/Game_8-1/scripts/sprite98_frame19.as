#version 4
if (Hit eq "1") {
    gotoAndPlay(115);
} else if (Miss eq "1") {
    gotoAndPlay(71);
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
