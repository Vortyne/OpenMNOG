#version 4
PlayRollover1 = "0";
GaKoro_GateOpen = eval("/:GaKoro_GateOpen");
if (GaKoro_GateOpen eq "1") {
    GateIsOpen = "1";
} else {
    GateIsOpen = "0";
}
GaKoro = eval("/:GaKoro");
if (GaKoro eq "1") {
    setProperty("Boat", _y, "2000");
}
