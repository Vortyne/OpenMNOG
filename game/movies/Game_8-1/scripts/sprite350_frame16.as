#version 4
if (Hit eq "1") {
    gotoAndPlay(80);
} else if (Miss eq "1") {
    gotoAndPlay(70);
} else {
    Attacking = "1";
    AttackTime = eval("../:FramesPassed") + agility * "5";
    tellTarget ("Rahi") {
        Change = "1";
        gotoAndPlay("Ready");
    }
    gotoAndPlay(26);
}
