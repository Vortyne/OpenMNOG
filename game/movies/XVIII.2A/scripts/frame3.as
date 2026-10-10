#version 4
if (eval("/:LiftLoc") eq "Down") {
    if (eval("/:Button1") eq "1") {
        if (eval("/:Button2") eq "1") {
            if (eval("/:Button3") eq "1") {
                gotoAndPlay(5);
            }
        }
    }
} else if (eval("/:LiftLoc") eq "Up") {
    if (eval("/:Button1") eq "0") {
        if (eval("/:Button2") eq "0") {
            if (eval("/:Button3") eq "0") {
                gotoAndPlay(5);
            }
        }
    }
}
