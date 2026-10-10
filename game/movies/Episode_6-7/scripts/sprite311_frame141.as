#version 4
if (eval("/:ExitLoop") eq "1") {
    gotoAndPlay(144);
} else {
    RandNum = random("5") + "1";
    if (RandNum eq "1") {
        gotoAndPlay(7);
    } else if (RandNum eq "2") {
        gotoAndPlay(37);
    } else {
        gotoAndPlay(1);
    }
}
