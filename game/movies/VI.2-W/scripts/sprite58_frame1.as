#version 4
PlayRollover1 = "0";
FindGaKoro_Cog = eval("/:FindGaKoro_Cog");
if ("9" < FindGaKoro_Cog and FindGaKoro_Cog < "13") {
    if (FindGaKoro_Cog eq "10") {
        setProperty("Cog", _x, getProperty("CogPlacement10", _x));
        setProperty("Cog", _y, getProperty("CogPlacement10", _y));
    } else if (FindGaKoro_Cog eq "11") {
        setProperty("Cog", _x, getProperty("CogPlacement11", _x));
        setProperty("Cog", _y, getProperty("CogPlacement11", _y));
    } else if (FindGaKoro_Cog eq "12") {
        setProperty("Cog", _x, getProperty("CogPlacement12", _x));
        setProperty("Cog", _y, getProperty("CogPlacement12", _y));
    }
}
stop();
