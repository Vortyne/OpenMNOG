#version 4
if (eval("/:LiftLoc") eq "Down") {
    tellTarget ("Pod") {
        gotoAndPlay("PlayUp");
    }
} else if (eval("/:LiftLoc") eq "Up") {
    tellTarget ("Pod") {
        gotoAndPlay("PlayDown");
    }
}
