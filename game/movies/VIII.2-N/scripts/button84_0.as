#version 4
tellTarget ("/") {
    QuarryKey = random("6") + "1";
}
tellTarget ("/Backpack/Backpack") {
    gotoAndPlay("ResetOrder");
}
