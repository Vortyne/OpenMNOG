#version 4
if (frameLoaded(5)) {
    tellTarget ("../") {
        Loaded = "1";
    }
    gotoAndStop(3);
}
