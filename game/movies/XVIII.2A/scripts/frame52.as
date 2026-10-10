#version 4
if (eval("/:LiftLoc") eq "Down") {
    tellTarget ("Pod") {
        gotoAndStop("Down");
    }
} else if (eval("/:LiftLoc") eq "Up") {
    tellTarget ("Pod") {
        gotoAndStop("Up");
    }
}
