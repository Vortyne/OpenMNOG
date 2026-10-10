#version 4
if (eval("/:LavaBoard") < "1" and eval("/:Backpack") eq "1") {
    tellTarget ("/") {
        LavaBoard = "1";
    }
    tellTarget ("/Backpack/Backpack") {
        gotoAndPlay("ResetOrder");
    }
    reload = "1";
}
