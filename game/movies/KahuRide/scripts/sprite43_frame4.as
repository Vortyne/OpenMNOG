#version 4
if (eval("../../../../../:DamageLevel") eq "2") {
    gotoAndPlay(5);
} else {
    gotoAndPlay(3);
}
