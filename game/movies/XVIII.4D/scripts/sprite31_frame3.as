#version 4
tellTarget ("/") {
    KahuRide = "1";
}
tellTarget ("/Backpack/Backpack") {
    gotoAndPlay("ResetOrder");
}
stop();
