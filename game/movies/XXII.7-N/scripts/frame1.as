#version 4
tellTarget ("/") {
    TargetMovie = "XXII.7-N.swf";
}
if (eval("/:Loc") eq "1-S") {
    gotoAndPlay(4);
} else if (eval("/:Loc") eq "2-N") {
    gotoAndPlay(7);
} else if (eval("/:Loc") eq "2-S") {
    gotoAndPlay(10);
} else if (eval("/:Loc") eq "3-N") {
    gotoAndPlay(13);
} else if (eval("/:Loc") eq "3-S") {
    gotoAndPlay(16);
}
tellTarget ("/") {
    Startedloading = "1";
}
