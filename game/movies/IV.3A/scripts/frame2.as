#version 4
if (frameLoaded(45)) {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    gotoAndPlay(5);
}
