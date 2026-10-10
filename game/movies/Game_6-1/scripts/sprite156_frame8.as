#version 4
if (eval("../../../../../:DamageLevel") eq "4") {
    set("../../../../../:Lose", "1");
    gotoAndPlay(7);
} else {
    gotoAndPlay(7);
}
