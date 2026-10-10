#version 4
PlayRollover1 = "0";
FindGaKoro_Cog = eval("../:FindGaKoro_Cog");
if ("0" < FindGaKoro_Cog and FindGaKoro_Cog < "4") {
    if (FindGaKoro_Cog eq "1") {
        setProperty("Cog", _x, getProperty("CogPlacement1", _x));
        setProperty("Cog", _y, getProperty("CogPlacement1", _y));
    } else if (FindGaKoro_Cog eq "2") {
        setProperty("Cog", _x, getProperty("CogPlacement2", _x));
        setProperty("Cog", _y, getProperty("CogPlacement2", _y));
    } else if (FindGaKoro_Cog eq "3") {
        setProperty("Cog", _x, getProperty("CogPlacement3", _x));
        setProperty("Cog", _y, getProperty("CogPlacement3", _y));
    }
}
