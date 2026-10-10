#version 4
if (eval("/:ExitLoop") eq "1") {
    gotoAndPlay(101);
} else {
    RandNum = random("8") + "1";
    if (RandNum eq "1") {
        gotoAndPlay(24);
    } else if (RandNum eq "2") {
        gotoAndPlay(1);
    } else if (RandNum eq "3") {
        gotoAndPlay(80);
    } else {
        prevFrame();
        play();
    }
}
