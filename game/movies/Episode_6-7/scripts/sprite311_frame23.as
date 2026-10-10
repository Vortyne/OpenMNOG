#version 4
if (eval("/:ExitLoop") eq "1") {
    gotoAndPlay(68);
} else {
    RandNum = random("12") + "1";
    if (RandNum eq "1") {
        gotoAndPlay(24);
    } else if (RandNum eq "2") {
        gotoAndPlay(68);
    } else {
        prevFrame();
        play();
    }
}
