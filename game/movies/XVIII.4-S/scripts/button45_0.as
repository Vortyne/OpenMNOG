#version 4
tellTarget ("/") {
    Flute = "1";
}
tellTarget ("/Backpack/Backpack") {
    gotoAndPlay("ResetOrder");
}
