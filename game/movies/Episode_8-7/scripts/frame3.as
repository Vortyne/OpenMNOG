#version 4
if (frameLoaded(116)) {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    gotoAndPlay(5);
}
