#version 4
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:cMaku") eq "1") {
    setProperty("Middle-Ground/MakuPickup", _x, "2000");
    setProperty("Middle-Ground/MakuBubble", _x, "2000");
    setProperty("Middle-Ground/Maku", _x, "2000");
} else if (eval("/:cKapura") eq "1") {
    setProperty("Middle-Ground/MakuBubble", _x, "2000");
} else {
    setProperty("Middle-Ground/MakuPickup", _x, "2000");
}
