#version 4
if (eval("../:AllowRollover") eq "1") {
    gotoAndPlay(3);
} else {
    gotoAndPlay(1);
}
