#version 5
if (Movieisloaded eq "1") {
    // (Loading screen: Slow: shown for at least 3 seconds.)
    if (eval("_level0:SlowLoadScreen") ne "1" or getTimer() - LoadStarted >= 3000) {
        gotoAndPlay(35);
    }
}
