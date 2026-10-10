#version 5
if (frameLoaded(164)) {
    tellTarget ("/") {
        Movieisloaded = 1;
    }
    gotoAndPlay(5);
}
