#version 4
set("../:ThrowName3", "");
set("../:ThrowTarget", "");
set("../:ThrowFrames", "");
setProperty("../" add ThrowName3, _x, "2000");
setProperty("../" add ThrowName3, _y, "0");
set("../:QueuePick", "1");
Attack = "0";
Attacking = "0";
PrepAttack = "0";
Target = "";
gotoAndPlay(4);
tellTarget ("Rahi") {
    gotoAndPlay("idle");
}
