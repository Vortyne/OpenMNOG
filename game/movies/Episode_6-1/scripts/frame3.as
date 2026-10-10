#version 5
if (frameLoaded(173)) {
    tellTarget ("/") {
        Movieisloaded = 1;
    }
    gotoAndPlay(5);
}
