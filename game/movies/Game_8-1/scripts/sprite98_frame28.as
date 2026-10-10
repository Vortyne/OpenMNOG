#version 4
Defend = "0";
if (eval("../" add Target add "/:Health") < "1") {
    if (Attack eq "1") {
        set("../:QueuePick", "1");
    }
    Attack = "0";
    AttackTime = "0";
    Attacking = "0";
    PrepAttack = "0";
    Target = "";
    tellTarget ("Rahi") {
        Change = "1";
        gotoAndPlay("idle");
    }
    gotoAndPlay(4);
} else if (Attack eq "1") {
    gotoAndPlay(34);
} else if (Hit eq "1") {
    gotoAndPlay(115);
} else if (Miss eq "1") {
    gotoAndPlay(71);
} else {
    gotoAndPlay(22);
}
