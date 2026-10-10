#version 4
GaKoro = eval("/:GaKoro");
if (GaKoro eq "1") {
    gotoAndPlay(4);
} else {
    tellTarget ("/") {
        Startedloading = "1";
        GaKoroVisited = "1";
    }
    loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder");
    GaKoro = eval("/:GaKoro");
    V1Svisited = eval("/:V1Svisited");
    if (GaKoro != "1" and V1Svisited != "1") {
        tellTarget ("/") {
            V1Svisited = "1";
        }
        loadMovie("SFX_Gakorosting.swf", "SoundFX");
    }
}
