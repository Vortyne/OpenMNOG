#version 4
tellTarget ("/") {
    Startedloading = "1";
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
if (eval("/:cHafu") eq "1") {
    setProperty("Middle-Ground/HafuPickup", _x, "2000");
    setProperty("Middle-Ground/HafuBubble", _x, "2000");
    setProperty("Middle-Ground/Hafu", _x, "2000");
    tellTarget ("Middle-Ground/Hafu") {
        gotoAndStop(1);
    }
} else if (eval("/:cKapura") eq "1") {
    setProperty("Middle-Ground/HafuBubble", _x, "2000");
} else {
    setProperty("Middle-Ground/HafuPickup", _x, "2000");
}
