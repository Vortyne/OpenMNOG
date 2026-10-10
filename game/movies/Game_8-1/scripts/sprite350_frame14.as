#version 4
if (Hit eq "1") {
    gotoAndPlay(80);
} else if (Miss eq "1") {
    gotoAndPlay(70);
} else if ("0" < eval("../" add Target add "/:Health")) {
    gotoAndPlay(16);
} else {
    gotoAndPlay(13);
}
