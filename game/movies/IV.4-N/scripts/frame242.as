#version 4
BridgeSide = eval("/:BridgeSide");
if (BridgeSide eq "IV3") {
    tellTarget ("/") {
        MovieName = "IV.3-W.swf";
        VerticalShift = "0";
        HorizontalShift = "0";
        Sound_ChangeBackgroundVolume = "1";
        Sound_BG_Volume = "Low";
        TaBridge = "0";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        MovieName = "IV.7-E.swf";
        VerticalShift = "0";
        HorizontalShift = "0";
        Sound_ChangeBackgroundVolume = "1";
        Sound_BG_Volume = "Low";
        TaBridge = "0";
        BeginLoadMovie = "1";
    }
}
stop();
