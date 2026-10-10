#version 4
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:FireStaff") eq "1") {
    setProperty("HeatStone", _x, "2000");
}
