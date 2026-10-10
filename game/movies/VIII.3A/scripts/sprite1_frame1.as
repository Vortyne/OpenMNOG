#version 4
Chisel = eval("/:Chisel");
Pokoro = eval("/:Pokoro");
if (Pokoro eq "1" and Chisel eq "0") {
    GiveItem = "1";
} else {
    GiveItem = "0";
}
