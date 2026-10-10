#version 4
PlayRollover1 = "0";
Startpump2 = "1";
GaKoro = eval("/:GaKoro");
GaKoro_Cog = eval("/:GaKoro_Cog");
GaKoro_Cog_Placed = eval("/:GaKoro_Cog_Placed");
if (GaKoro eq "0") {
    if (GaKoro_Cog_Placed eq "1") {
    } else {
        Wasntplaced = "1";
        setProperty("MainCog", _x, "3000");
    }
}
