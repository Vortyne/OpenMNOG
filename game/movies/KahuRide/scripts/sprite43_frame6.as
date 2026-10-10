#version 4
if (eval("../../../../../:DamageLevel") eq "3") {
    gotoAndPlay(5);
} else {
    gotoAndPlay(7);
}
