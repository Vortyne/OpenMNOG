#version 4
tellTarget ("/") {
    Startedloading = "1";
}
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder", "GET");
if (eval("/:cHafu") eq "1") {
    setProperty("Back-Ground/Hafu", _x, "2000");
    tellTarget ("Back-Ground/Hafu") {
        gotoAndStop(1);
    }
}
