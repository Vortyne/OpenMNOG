#version 4
if (PlayLow eq "1") {
    PlayLow = "0";
    PlayHigh = "1";
    gotoAndPlay(6);
} else if (PlayHigh eq "1") {
    PlayLow = "1";
    PlayHigh = "0";
    gotoAndPlay(12);
}
