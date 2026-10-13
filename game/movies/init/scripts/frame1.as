#version 6
_level0.__matanui.util = {_: _level0.__matanui};
_level0.__matanui.util._whitespace = [];
_level0.__matanui.util._whitespace[0] = true;
_level0.__matanui.util._whitespace[9] = true;
_level0.__matanui.util._whitespace[10] = true;
_level0.__matanui.util._whitespace[13] = true;
_level0.__matanui.util._whitespace[32] = true;
_level0.__matanui.util.trimStart = function(s) {
    var w = this._whitespace;
    var i = 0;
    while (i < s.length && w[s.charCodeAt(i)]) {
        i++;
    }
    return s.substr(i);
};
_level0.__matanui.util.trimEnd = function(s) {
    var w = this._whitespace;
    var i = s.length;
    while (0 < i && w[s.charCodeAt(i - 1)]) {
        i--;
    }
    return s.substr(0, i);
};
_level0.__matanui.util.trim = function(s) {
    return this.trimEnd(this.trimStart(s));
};
_level0.__matanui.util.truthy = function(v) {
    return typeof v != "string" ? !!v : !!v.length;
};
_level0.__matanui.util.encodeQuery = function(s) {
    var r = "";
    var i = 0;
    while (i < s.length) {
        var c = s.charCodeAt(i);
        if (!(c >= 97 && 122 >= c || c >= 65 && 90 >= c || c >= 48 && 57 >= c || c == 33 || c >= 39 && 42 >= c || c >= 45 && 46 >= c || c == 95 || c == 126)) {
            r = r + ("%" + (15 < c ? "" : "0") + c.toString(16).toUpperCase());
        } else {
            r = r + s.charAt(i);
        }
        i++;
    }
    return r;
};
_level0.__matanui.util.fscommand = function(k, v) {
    getURL("FSCommand:" + k, v);
};
_level0.__matanui.save = {_: _level0.__matanui};
_level0.__matanui.save._prefix = "matanui_";
_level0.__matanui.save._lso = function() {
    return SharedObject.getLocal("mata-nui-online-game", "/");
};
_level0.__matanui.save._settings = [];
_level0.__matanui.save._setting = function(key, clean) {
    var r = {
        _: this,
        _key: key,
        _clean: clean,
        _value: !this._.LSO ? _level0[this._prefix + key] : this._lso().data[key],
        get: function() {
            return !this._._.util.truthy(this._value) ? this._clean : this._value;
        },
        set: function(value) {
            this._value = value;
            if (!this._._.LSO) {
                this._._.util.fscommand(this._._prefix + this._key, value);
            } else {
                var so = this._._lso();
                so.data[this._key] = value;
                so.flush();
            }
        },
        clear: function() {
            this.set(this._clean);
        },
        clean: function() {
            return this._clean;
        }
    };
    this._settings.push([key, r]);
    return r;
};
_level0.__matanui.save.query = function() {
    var params = [];
    var i = 0;
    while (i < this._settings.length) {
        var setting = this._settings[i];
        params.push(this._.util.encodeQuery(this._prefix + setting[0]) + "=" + this._.util.encodeQuery(setting[1].get()));
        i++;
    }
    return params.join("&");
};
_level0.__matanui.save.date = _level0.__matanui.save._setting("date", "1007406064");
_level0.__matanui.save.language = _level0.__matanui.save._setting("language", "eng");
_level0.__matanui.save.fps = _level0.__matanui.save._setting("fps", "18");
_level0.__matanui.save.state = _level0.__matanui.save._setting("state", "&nostate=1&");
_level0.__matanui.getdate = function(holder) {
    holder.ServerBasedDate = this.save.date.get();
};
_level0.__matanui.getlanguage = function(holder) {
    holder.Language = this.save.language.get();
};
_level0.__matanui.getstate = function(holder) {
    var params = this.save.state.get().split("&");
    var i = 0;
    while (i < params.length) {
        var param = params[i].split("=");
        holder[param[0]] = param[1];
        i++;
    }
};
_level0.__matanui.setstate = function(holder) {
    var value = "&";
    for ($r0 in holder) {
        var p = $r0;
        var v = holder[p];
        value = value + (p + "=" + (typeof v != "undefined" ? v : "") + "&");
    }
    this.save.state.set(value);
};
_level0.__matanui.setClipboard = function(str) {
    System.setClipboard(str);
};
_level0.__matanui.toggleFullscreen = function() {
    Stage.displayState = Stage.displayState != "normal" ? "normal" : "fullScreen";
};
_level0.__matanui.reload = function() {
    loadMovie("player.swf", "_level1");
};
_level0.__matanui.reinit = function() {
    var url = this.save.fps.get() != "30" ? "matanuionlinegame.swf" : "matanuionlinegame-30fps.swf";
    if (!this.LSO) {
        url = url + ("?" + this.save.query());
    }
    loadMovie(url, "_level0");
};
_level0.__matanui.main = function() {
    if (this.FPS != this.save.fps.get()) {
        this.reinit();
        return;
    }
    loadMovie("overlay.swf", "_level10001");
};
_level0.__matanui.save.widescreen = _level0.__matanui.save._setting("widescreen", "1");
_level0.__matanui.applyWidescreen = function() {
    var on = this.save.widescreen.get() == "1";
    this.util.fscommand("widescreen", on ? "1" : "0");
    _level10001.matanui_widescreen_left._visible = !on;
    _level10001.matanui_widescreen_right._visible = !on;
    _level10001.matanui_overlay.matanui_border.matanui_widescreen_bar._visible = on;
};
_level0.__matanui.toggleWidescreen = function() {
    this.save.widescreen.set(this.save.widescreen.get() == "1" ? "0" : "1");
    this.applyWidescreen();
};
_level0.__matanui.save.smooth = _level0.__matanui.save._setting("smooth", "1");
_level0.__matanui.applySmooth = function() {
    // 0 off, 1 partial (the default), 2 full.
    var v = this.save.smooth.get();
    this.util.fscommand("smooth", v == "0" || v == "2" ? v : "1");
};
_level0.__matanui.setSmooth = function(v) {
    this.save.smooth.set(v);
    this.applySmooth();
};
// The Smoothing button cycles Off, Partial, Full.
_level0.__matanui.cycleSmooth = function() {
    var v = this.save.smooth.get();
    this.setSmooth(v == "0" ? "1" : (v == "1" ? "2" : "0"));
};
_level0.__matanui.save.debug = _level0.__matanui.save._setting("debug", "0");
_level0.__matanui.applyDebug = function() {
    this.util.fscommand("debug", this.save.debug.get() == "1" ? "1" : "0");
};
_level0.__matanui.toggleDebug = function() {
    this.save.debug.set(this.save.debug.get() == "1" ? "0" : "1");
    this.applyDebug();
};
// The game always runs at its original 18 fps (Project Brick's 30 fps option breaks many scenes'
// timing); a save that chose 30 is set back before main() would switch to the 30 fps build.
_level0.__matanui.save.fps.set("18");
_level0.__matanui.setLanguage = function(v) {
    // Text is loaded in the player's Language as it's needed, so switching it takes effect from the
    // next dialog or scene; the backpack's item names are loaded once, so they're reloaded here.
    this.save.language.set(v);
    _level1.Language = v;
    loadVariables("Text_backpack_" + v + ".txt", "_level1.Backpack.Backpack");
};
// Title screen (movies/title on _level20000): shown at startup instead of loading the player straight
// away; its buttons start the game, which fades it out once the first scene is up.
_level0.__matanui.save.title = _level0.__matanui.save._setting("title", "1");
_level0.__matanui.toggleTitle = function() {
    this.save.title.set(this.save.title.get() == "1" ? "0" : "1");
};
// Loading screen: Off goes from scene to scene directly, without the fade to black and the globe
// (the player's frames skip them; the engine keeps the scene lit and the globe hidden meanwhile).
// Slow ("2") shows it for at least 3 seconds (the player's frame 33 waits).
_level0.__matanui.save.loadscreen = _level0.__matanui.save._setting("loadscreen", "1");
_level0.__matanui.applyLoadscreen = function() {
    var v = this.save.loadscreen.get();
    var on = v != "0";
    _level0.SkipLoadScreen = on ? "0" : "1";
    _level0.SlowLoadScreen = v == "2" ? "1" : "0";
    this.util.fscommand("loadscreen", on ? "1" : "0");
};
_level0.__matanui.toggleLoadscreen = function() {
    // On, Slow, Off.
    var v = this.save.loadscreen.get();
    this.save.loadscreen.set(v == "1" ? "2" : (v == "2" ? "0" : "1"));
    this.applyLoadscreen();
};
// Save slots (1-12; the engine's save slot menu): the game saves to save.state, which is the current
// slot's setting. Slot 1 is the original "state", so an existing save becomes slot 1.
_level0.__matanui.save.slot = _level0.__matanui.save._setting("slot", "1");
_level0.__matanui.stateKey = function(n) {
    return n == "1" ? "state" : "state" + n;
};
_level0.__matanui.useSlot = function(n) {
    this.save.slot.set(n);
    this.save.state = this.save._setting(this.stateKey(n), "&nostate=1&");
};
_level0.__matanui.deleteSlot = function(n) {
    var s = this.save._setting(this.stateKey(n), "&nostate=1&");
    s.clear();
    if (n == this.save.slot.get()) {
        this.save.state = s;
    }
};
if (_level0.__matanui.save.slot.get() != "1") {
    _level0.__matanui.save.state = _level0.__matanui.save._setting(_level0.__matanui.stateKey(_level0.__matanui.save.slot.get()), "&nostate=1&");
}
_level0.__matanui.hasSave = function() {
    return this.save.state.get() != "&nostate=1&";
};
_level0.__matanui.start = function() {
    // Called by the overlay once it's up: the title screen, or the game as before.
    if (this.save.title.get() == "1" && !this.titleShown) {
        this.titleShown = true;
        _level10001.matanui_overlay.matanui_border._visible = false;
        loadMovie("title.swf", "_level20000");
    } else {
        this.reload();
    }
};
_level0.__matanui.startFromTitle = function(newGame) {
    if (_level20000.leaving == 1) {
        return;
    }
    if (_level20000.confirm != undefined) {
        _level20000.confirm.removeMovieClip();
    }
    // Continuing goes straight to the game (with its loading screen); a new game starts on the
    // title's own beach view, which fades into the game's below.
    if (!newGame && this.hasSave()) {
        this.reload();
        this.titleDone();
        return;
    }
    this.save.state.clear();
    this.reload();
    _level20000.leaving = 1;
    // The menu bar is back beneath the title's sand strip, which fades into it.
    _level10001.matanui_overlay.matanui_border._visible = true;
    this.util.fscommand("fadeout", "20000");
};
_level0.__matanui.titleDone = function() {
    _level20000.unloadMovie();
    _level10001.matanui_overlay.matanui_border._visible = true;
};
_level0.__matanui.toTitle = function() {
    // Back to the title screen from the game (the engine's prompt after the last episode).
    _level1.unloadMovie();
    stopAllSounds();
    this.titleShown = false;
    this.start();
};
_level0.__matanui.save.textspeed = _level0.__matanui.save._setting("textspeed", "0");
_level0.__matanui.applyTextspeed = function() {
    // Letters the dialog adds per frame (TextSub reads _level0.TextStep); the original adds 2.
    var v = this.save.textspeed.get();
    _level0.TextStep = v == "2" ? "100000" : (v == "1" ? "4" : "2");
    // How far the question box slides each frame (TextSub's HeightStep); the original moves 4.
    _level0.QBoxStep = v == "2" ? "64" : (v == "1" ? "8" : "4");
    // How much bigger the conversation dialog's slide-in steps are (see Patches.DialogSlide).
    _level0.SlideFactor = v == "2" ? "1000" : (v == "1" ? "2" : "1");
};
_level0.__matanui.cycleTextspeed = function() {
    var v = this.save.textspeed.get();
    this.save.textspeed.set(v == "0" ? "1" : (v == "1" ? "2" : "0"));
    this.applyTextspeed();
};
// Panning (Misc > Panning): the speed (percent of the original's) and how far from the view's edge
// the mouse starts it (the original's 120); the player's panning calls panScene (PanSettings).
// Vector sand (Misc): the beach scenes' photographic sand drawn as vector art (the engine's vector images).
_level0.__matanui.save.vectorsand = _level0.__matanui.save._setting("vectorsand", "1");
_level0.__matanui.applyVectorsand = function() {
    this.util.fscommand("vectorsand", this.save.vectorsand.get() == "0" ? "0" : "1");
};
_level0.__matanui.toggleVectorsand = function() {
    this.save.vectorsand.set(this.save.vectorsand.get() == "0" ? "1" : "0");
    this.applyVectorsand();
};
// Textbox transparency (Graphics): the conversations' text boxes see-through, as the original's (Normal, "1"), or much
// less (Low, "0").
_level0.__matanui.save.textboxalpha = _level0.__matanui.save._setting("textboxalpha", "1");
_level0.__matanui.applyTextboxalpha = function() {
    this.util.fscommand("textboxalpha", this.save.textboxalpha.get() == "0" ? "0" : "1");
};
_level0.__matanui.toggleTextboxalpha = function() {
    this.save.textboxalpha.set(this.save.textboxalpha.get() == "0" ? "1" : "0");
    this.applyTextboxalpha();
};
_level0.__matanui.save.panspeed = _level0.__matanui.save._setting("panspeed", "100");
_level0.__matanui.save.panzone = _level0.__matanui.save._setting("panzone", "120");
_level0.__matanui.cyclePanspeed = function() {
    var v = this.save.panspeed.get();
    this.save.panspeed.set(v == "50" ? "75" : (v == "75" ? "100" : (v == "100" ? "150" : "50")));
};
_level0.__matanui.cyclePanzone = function() {
    var v = this.save.panzone.get();
    this.save.panzone.set(v == "60" ? "90" : (v == "90" ? "120" : (v == "120" ? "0" : "60")));
};
// Choices since removed (Fastest, Large) become the nearest left.
if (_level0.__matanui.save.panspeed.get() == "200") {
    _level0.__matanui.save.panspeed.set("150");
}
if (_level0.__matanui.save.panzone.get() == "180") {
    _level0.__matanui.save.panzone.set("120");
}
_level0.__matanui.panBand = function(d, zone) {
    // Edge zone Off (0): the mouse doesn't pan, only the keys do.
    if (zone <= 0) {
        return 0;
    }
    // The original's bands: within the zone 10 a frame, within half of it 20, within a sixth 30.
    return d < zone / 6 ? 30 : (d < zone / 2 ? 20 : (d < zone ? 10 : 0));
};
_level0.__matanui.panScene = function() {
    var p = _level1;
    if (p.ShiftOff == "1" || p.FluteNoShift == "1") {
        return;
    }
    // Not while a menu covers the scene (the Help, settings, save slots...).
    if (System.mnogMenuOpen()) {
        return;
    }
    var horizontal = p.VerticalShift == "0" && p.HorizontalShift == "1";
    var vertical = p.VerticalShift == "1" && p.HorizontalShift == "0";
    if (!horizontal && !vertical) {
        return;
    }
    var zone = Number(this.save.panzone.get());
    var speed = Number(this.save.panspeed.get()) / 100;
    // (A scene can slow its own panning: PanSpeed on the scene, e.g. "0.5" for half the setting's speed, where its
    // close foreground otherwise races past, as in XV.2-S.)
    var own = Number(p.holder.PanSpeed);
    if (own > 0) {
        speed = speed * own;
    }
    var mx = p.mousedrag._x;
    var my = p.mousedrag._y;
    // (By slash paths: _level1's Backpack is also a variable, "1" once the backpack's found, which p.Backpack gives
    // instead of the clip. The party's bar: the companion chosen, top right.)
    var open = eval("_level1/Backpack/:BackpackOpen") == "1";
    var party = eval("_level1/Backpack/cBar/:cBarOpen") == "1";
    var mg = p.holder["Middle-Ground"];
    var fg = p.holder["Fore-Ground"];
    var bg = p.holder["Back-Ground"];
    var step = 0;
    if (horizontal) {
        // Not over the backpack's tab (top of the view), as in the original.
        if (my > 40 + (open ? 100 : 0) || p.Backpack != "1") {
            if (mg._x > 30) {
                step = step - this.panBand(_level0.ViewRight - mx, zone);
            }
            if (mg._x < 740) {
                step = step + this.panBand(mx - _level0.ViewLeft, zone);
            }
        }
        // Arrow keys and A/D, at the fastest band's speed.
        if (mg._x > 30 && (Key.isDown(39) || Key.isDown(68))) {
            step = step - 30;
        }
        if (mg._x < 740 && (Key.isDown(37) || Key.isDown(65))) {
            step = step + 30;
        }
        step = step * speed;
        if (step != 0) {
            mg._x = mg._x + step;
            fg._x = fg._x + step * 2;
            bg._x = bg._x + step / 2;
            var c = mg._x < 30 ? 30 - mg._x : (mg._x > 740 ? 740 - mg._x : 0);
            mg._x = mg._x + c;
            fg._x = fg._x + c * 2;
            bg._x = bg._x + c / 2;
        }
    } else {
        // (Not at all while the items or the party are open, over the top of the view: neither the mouse nor the keys
        // move it under them.)
        if (!open && !party) {
            // (Not up from the top right, where the party and bag buttons are: the original kept clear of x 720 on,
            // which still left the party button (from x 692) and half the bag panning the view as they were pointed at.)
            if (mg._y < 395 && mx < 670) {
                step = step + this.panBand(my, zone);
            }
            if (mg._y > 30) {
                step = step - this.panBand(425 - my, zone);
            }
            // Arrow keys and W/S.
            if (mg._y < 395 && (Key.isDown(38) || Key.isDown(87))) {
                step = step + 30;
            }
            if (mg._y > 30 && (Key.isDown(40) || Key.isDown(83))) {
                step = step - 30;
            }
        }
        step = step * speed;
        if (step != 0) {
            mg._y = mg._y + step;
            fg._y = fg._y + step * 2;
            bg._y = bg._y + step / 2;
            var c = mg._y < 30 ? 30 - mg._y : (mg._y > 395 ? 395 - mg._y : 0);
            mg._y = mg._y + c;
            fg._y = fg._y + c * 2;
            bg._y = bg._y + c / 2;
        }
    }
};
_level0.__matanui.clampPanorama = function() {
    if (this.save.widescreen.get() != "1") {
        return;
    }
    // Scenes the player pans sideways, or up and down; some (the Charred Forest maze) scroll and wrap themselves.
    var vertical = _level1.VerticalShift == "1" && _level1.HorizontalShift != "1";
    var sideways = _level1.VerticalShift == "0" && _level1.HorizontalShift == "1";
    if (_level1.ShiftOff == "1" || _level1.FluteNoShift == "1" || !(vertical || sideways)) {
        return;
    }
    var holder = _level1.holder;
    var names = ["Back-Ground", "Middle-Ground", "Fore-Ground"];
    var rates = [0.5, 1, 2];
    var layers = [];
    var layerRates = [];
    var i = 0;
    while (i < 3) {
        var layer = holder[names[i]];
        if (layer != undefined) {
            layers.push(layer);
            layerRates.push(rates[i]);
        }
        i++;
    }
    Stage.mnogClampScenery(layers, layerRates, vertical);
};
_level0.__matanui.main();
