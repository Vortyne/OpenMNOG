#version 4
if (Hit eq "1") {
    gotoAndPlay(120);
} else if (Miss eq "1") {
    gotoAndPlay(105);
} else if ("0" < eval("../" add Target add "/:Health")) {
    gotoAndPlay(16);
} else {
    gotoAndPlay(13);
}
