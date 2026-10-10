#version 4
PlayRollover1 = "0";
FindGaKoro_Cog = eval("/:FindGaKoro_Cog");
if ("6" < FindGaKoro_Cog and FindGaKoro_Cog < "10") {
    if (FindGaKoro_Cog eq "7") {
        setProperty("Cog", _x, getProperty("CogPlacement7", _x));
        setProperty("Cog", _y, getProperty("CogPlacement7", _y));
    } else if (FindGaKoro_Cog eq "8") {
        setProperty("Cog", _x, getProperty("CogPlacement8", _x));
        setProperty("Cog", _y, getProperty("CogPlacement8", _y));
    } else if (FindGaKoro_Cog eq "9") {
        setProperty("Cog", _x, getProperty("CogPlacement9", _x));
        setProperty("Cog", _y, getProperty("CogPlacement9", _y));
    }
}
stop();
