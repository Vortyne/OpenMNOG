#version 6
if (leaving != 1 && ui._alpha < 100) {
    ui._alpha = Math.min(100, ui._alpha + 100 / 36);
}
if (leaving == 1) {
    fade = fade + 1;
    ui._alpha = Math.min(ui._alpha, Math.max(0, 100 - fade * 4));
    // The game's menu bar is beneath the sand strip.
    sand._alpha = Math.max(0, 100 - fade * 4);
    // The game is ready once its first scene is loaded and its loading screen and fade-in
    // (the player's PlayMovie frames, 35-43) are over: it is back in its play loop (19-23).
    var ready = _level1.Movieisloaded == "1" && _level1._currentframe >= 19 && _level1._currentframe <= 23;
    if (fade >= 25 && ready) {
        _level0.__matanui.titleDone();
    }
}
