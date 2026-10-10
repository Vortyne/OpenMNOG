#version 4
tellTarget ("/") {
    Startedloading = "1";
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
if (eval("/:cKapura") eq "0" and eval("/:fGaKoro") eq "10" and eval("/:fTaKoro") eq "10" and eval("/:fOnuKoro") eq "10" and eval("/:fPoKoro") eq "10" and eval("/:fLeKoro") eq "10" and eval("/:fKoKoro") eq "10") {
} else {
    setProperty("Middle-Ground/Kapura", _x, "2000");
}
