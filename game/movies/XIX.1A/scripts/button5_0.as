#version 4
tellTarget ("/") {
    FireStaff = "1";
}
setProperty("../HeatStone", _x, "2000");
tellTarget ("/Backpack/Backpack") {
    gotoAndPlay("ResetOrder");
}
