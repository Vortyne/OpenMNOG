#version 4
if (frameLoaded(774)) {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    gotoAndPlay(3);
    loadMovie("blank", "/Music-holder/Intro-holder");
}
