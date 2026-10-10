#version 4
if (eval("/:fGaKoro") eq "10") {
    tellTarget ("Middle-Ground/Fortifications") {
        gotoAndStop(2);
    }
    tellTarget ("Back-Ground/Fortifications") {
        gotoAndStop(2);
    }
}
GaKoro = eval("/:GaKoro");
if (GaKoro eq "1") {
    setProperty("/holder/Back-Ground/Steam1", _y, "2000");
}
tellTarget ("/") {
    Startedloading = "1";
}
Rollover1Playing = "0";
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
if (eval("/:cKapura") eq "0" and eval("/:fGaKoro") eq "10" and eval("/:fTaKoro") eq "10" and eval("/:fOnuKoro") eq "10" and eval("/:fPoKoro") eq "10" and eval("/:fLeKoro") eq "10" and eval("/:fKoKoro") eq "10") {
} else {
    setProperty("Middle-Ground/Kapura", _x, "2000");
}
