#version 4
setProperty("/holder/Middle-Ground/Cog", _y, "-2000");
tellTarget ("/") {
    GaKoro_Cog = "1";
    FindGaKoro_Cog = "0";
}
tellTarget ("/Backpack/Backpack") {
    gotoAndPlay("ResetOrder");
}
