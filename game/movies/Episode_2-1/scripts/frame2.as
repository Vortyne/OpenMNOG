#version 4
if (frameLoaded(757)) {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    gotoAndPlay(5);
}
