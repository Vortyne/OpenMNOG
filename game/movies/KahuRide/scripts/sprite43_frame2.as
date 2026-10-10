#version 4
if (eval("../../../../../:DamageLevel") eq "1") {
    gotoAndPlay(3);
} else {
    gotoAndPlay(1);
}
