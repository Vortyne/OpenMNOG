#version 4
if (frameLoaded(1)) {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    if (eval("_level0:KewaRideSeen") eq "1") {
        gotoAndPlay(287);
    } else {
        set("_level0:KewaRideSeen", "1");
        gotoAndPlay(5);
    }
}
