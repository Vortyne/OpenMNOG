#version 4
set("../:Target", Target);
set("../:HitCalc", "1");
tellTarget ("../" add ThrowName) {
    gotoAndPlay("Hit");
}
