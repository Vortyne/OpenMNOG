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
}
tellTarget ("/") {
    Startedloading = "1";
}
