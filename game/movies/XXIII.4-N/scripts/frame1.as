#version 4
tellTarget ("/") {
    TargetMovie = "XXIII.4-N.swf";
}
if (eval("/:Loc") eq "1-S") {
    gotoAndPlay(4);
} else if (eval("/:Loc") eq "2-N") {
    gotoAndPlay(7);
} else if (eval("/:Loc") eq "2-S") {
    gotoAndPlay(10);
}
tellTarget ("/") {
    Startedloading = "1";
}
