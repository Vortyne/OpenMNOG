#version 4
if (frameLoaded(292)) {
    Loaded = "1";
}
if (Loaded eq "1") {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    gotoAndPlay(4);
} else {
    gotoAndPlay(2);
}
