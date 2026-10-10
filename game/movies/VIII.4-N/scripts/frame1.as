#version 4
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:Pokoro") eq "1") {
    gotoAndPlay(4);
} else {
    loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
    if (eval("/:Episode4_1Visited") eq "1") {
        tellTarget ("/") {
            Episode4_1Visited = "0";
            KohliBall = "1";
        }
        tellTarget ("/Backpack/Backpack") {
            gotoAndPlay("ResetOrder");
        }
    }
}
