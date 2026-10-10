#version 4
if (eval("/:LiftLoc") eq "Up") {
    gotoAndPlay(1);
} else if (eval("/:LiftLoc") eq "Down") {
    gotoAndPlay(222);
}
tellTarget ("/") {
    Startedloading = "1";
}
