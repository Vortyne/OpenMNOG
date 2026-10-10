#version 4
if (frameLoaded(564)) {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    gotoAndPlay(5);
}
