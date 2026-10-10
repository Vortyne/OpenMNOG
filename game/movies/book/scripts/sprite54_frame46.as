#version 4
if (HasShifted eq "1") {
    tellTarget ("Blank") {
        // (The fade itself, past "Fadein"'s own frame and its stop(): scrolling now reaches a picture before the
    // cover has finished coming down and stopped there.)
    gotoAndPlay(7);
    }
} else {
    tellTarget ("Blank") {
        gotoAndStop("Fadeout");
    }
    HasShifted = "1";
    stop();
}
