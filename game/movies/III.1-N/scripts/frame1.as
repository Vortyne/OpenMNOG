#version 4
loadMovie("sky2.swf", "/holder/Back-Ground/Sky-holder");
I4WVisited = eval("/:I4WVisited");
if (I4WVisited eq "1") {
    setProperty("/holder/Middle-Ground/TahuButton", _y, "2000");
}
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:cKapura") eq "0" and eval("/:fGaKoro") eq "10" and eval("/:fTaKoro") eq "10" and eval("/:fOnuKoro") eq "10" and eval("/:fPoKoro") eq "10" and eval("/:fLeKoro") eq "10" and eval("/:fKoKoro") eq "10") {
} else {
    setProperty("Middle-Ground/Kapura", _x, "2000");
}
