#version 4
if (eval("/:LiftLoc") eq "Up") {
    tellTarget ("BG") {
        gotoAndStop("Up");
    }
    set("/:Button1", "1");
    set("/:Button2", "1");
    set("/:Button3", "1");
    tellTarget ("Button1") {
        gotoAndPlay("Up");
    }
    tellTarget ("Button2") {
        gotoAndPlay("Up");
    }
    tellTarget ("Button3") {
        gotoAndPlay("Up");
    }
} else {
    set("/:LiftLoc", "Down");
    // (OpenMNOG: the buttons start unpressed, as drawn: Button1-3 are shared with other scenes (XV.3A's puzzle leaves its
    // nine at "1"), and left at "1" they sent the lift up as soon as it was reached.)
    set("/:Button1", "0");
    set("/:Button2", "0");
    set("/:Button3", "0");
}
tellTarget ("/") {
    Startedloading = "1";
}
