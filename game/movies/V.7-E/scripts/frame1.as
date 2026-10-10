#version 4
GaKoro = eval("/:GaKoro");
Jewel = eval("/:Jewel");
if (GaKoro eq "1") {
    gotoAndPlay(3);
} else if (Jewel != "1") {
    setProperty("MachineFix", _x, "3500");
}
tellTarget ("/") {
    Startedloading = "1";
}
Rollover1Playing = "0";
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
