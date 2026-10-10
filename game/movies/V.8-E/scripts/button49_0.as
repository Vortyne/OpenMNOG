#version 4
Backpack = eval("/:Backpack");
if (Backpack eq "1") {
    tellTarget ("/") {
        Item_Lightstone = "1";
    }
    tellTarget ("/Backpack/Backpack") {
        gotoAndPlay("ResetOrder");
    }
} else {
    gotoAndPlay(3);
}
