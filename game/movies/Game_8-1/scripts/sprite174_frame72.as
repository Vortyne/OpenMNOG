#version 4
setProperty("../" add ThrowName, _x, "2000");
setProperty("../" add ThrowName, _y, "0");
set("../:QueuePick", "1");
Attack = "0";
Attacking = "0";
PrepAttack = "0";
Target = "";
gotoAndPlay(4);
tellTarget ("Rahi") {
    gotoAndPlay("idle");
}
