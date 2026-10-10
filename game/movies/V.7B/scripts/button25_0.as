#version 4
tellTarget ("LeftCog") {
    gotoAndPlay("Play");
}
tellTarget ("SoundFX") {
    gotoAndPlay("startfail");
}
if (GaKoro_Cog_Placed eq "1" and DontClick != "1") {
    DontClick = "1";
    tellTarget ("MainCog") {
        gotoAndPlay("Play");
    }
    tellTarget ("RightCog") {
        gotoAndPlay("Play");
    }
    tellTarget ("Pump1") {
        gotoAndPlay("Play");
    }
    setProperty("../Buttons", _xscale, "-2000");
    tellTarget ("SoundFX") {
        gotoAndPlay("pump");
    }
}
