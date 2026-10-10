#version 4
if (eval("/:OnuKoroDoor") eq "1") {
    loadedthatframe = "0";
    if (frameLoaded(224)) {
        loadedthatframe = "1";
    }
    if (loadedthatframe eq "1") {
        gotoAndPlay(224);
    } else {
        gotoAndPlay(1);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
