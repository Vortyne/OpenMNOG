#version 4
if (frameLoaded(6)) {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    loaded = "1";
}
