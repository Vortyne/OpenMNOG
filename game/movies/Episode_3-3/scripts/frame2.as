#version 4
if (frameLoaded(1245)) {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    gotoAndPlay(5);
}
