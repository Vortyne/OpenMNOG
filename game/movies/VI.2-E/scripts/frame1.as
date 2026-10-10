#version 4
Jewel = eval("/:Jewel");
GaKoro = eval("/:GaKoro");
if (GaKoro eq "1") {
    gotoAndPlay(3);
} else {
    if (Jewel eq "1") {
        setProperty("/holder/Middle-Ground/Jewel", _y, "-2000");
    }
    tellTarget ("/") {
        Startedloading = "1";
    }
}
