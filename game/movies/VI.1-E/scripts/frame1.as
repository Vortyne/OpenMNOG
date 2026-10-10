#version 4
GaKoro = eval("/:GaKoro");
GaKoro_Cog = eval("/:GaKoro_Cog");
Item_Lightstone = eval("/:Item_Lightstone");
if (GaKoro eq "1") {
    setProperty("/holder/Middle-Ground/Hut_Sunk", _y, "2000");
} else {
    setProperty("/holder/Middle-Ground/Hut_Up", _y, "2000");
}
tellTarget ("/") {
    Startedloading = "1";
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
if (GaKoro eq "0" and GaKoro_Cog eq "0" and Item_Lightstone eq "1") {
    FindGaKoro_Cog = eval("/:FindGaKoro_Cog");
    if ("0" < FindGaKoro_Cog) {
    } else {
        tellTarget ("/") {
            FindGaKoro_Cog = random("12") + "1";
        }
        FindGaKoro_Cog = eval("/:FindGaKoro_Cog");
    }
}
V6Visited = eval("/:V6Visited");
if (V6Visited eq "1") {
    V6Visited = "0";
    loadMovie("SFX_Spelunk.swf", "SoundFX");
}
