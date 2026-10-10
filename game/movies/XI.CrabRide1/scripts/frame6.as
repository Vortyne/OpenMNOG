#version 4
if (frameLoaded(68)) {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    frameloaded = "1";
}
if (frameloaded eq "1") {
} else {
    gotoAndPlay(5);
}
