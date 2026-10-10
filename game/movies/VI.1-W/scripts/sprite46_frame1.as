#version 4
PlayRollover1 = "0";
FindGaKoro_Cog = eval("/:FindGaKoro_Cog");
if ("3" < FindGaKoro_Cog and FindGaKoro_Cog < "7") {
    if (FindGaKoro_Cog eq "4") {
        setProperty("Cog", _x, getProperty("CogPlacement4", _x));
        setProperty("Cog", _y, getProperty("CogPlacement4", _y));
    } else if (FindGaKoro_Cog eq "5") {
        setProperty("Cog", _x, getProperty("CogPlacement5", _x));
        setProperty("Cog", _y, getProperty("CogPlacement5", _y));
    } else if (FindGaKoro_Cog eq "6") {
        setProperty("Cog", _x, getProperty("CogPlacement6", _x));
        setProperty("Cog", _y, getProperty("CogPlacement6", _y));
    }
}
stop();
