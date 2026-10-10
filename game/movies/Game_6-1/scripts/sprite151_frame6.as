#version 4
if (eval("../../../../../:DamageLevel") eq "3") {
    gotoAndPlay(7);
} else {
    gotoAndPlay(5);
}
