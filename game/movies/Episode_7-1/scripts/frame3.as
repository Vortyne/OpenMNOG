#version 4
if (frameLoaded(731)) {
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    if (eval("/:GondolaFrom") eq "Ta-Koro") {
        gotoAndPlay(5);
    } else {
        gotoAndPlay(430);
    }
}
