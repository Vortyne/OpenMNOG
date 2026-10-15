# MNOG script (`.as`)

The game's scripts are a small ActionScript 1 dialect: every piece of the original Flash bytecode was decompiled into
it once, reading like the original source, each construct behaving exactly as the bytecode it came from. They're
edited directly now (`game/movies/<Name>/scripts/*.as`). The engine compiles each script to its own bytecode when its
movie loads (`src/Mnog.Engine/Script/Compiler.cs`) and runs that in its interpreter (`Interpreter.cs`), with Flash's
rules for values: an edit takes effect the next time the game starts, with no build step.

```actionscript
#version 4
if (Quest eq "7") {
    BookOpen = "1";
    loadMovie(BookMovieName, "/holder");
} else {
    loadMovie(MovieName, "/holder");
}
tellTarget ("/mousedrag") {
    gotoAndPlay("mouseblank");
}
```

## Header

- `#version N` — the SWF version of the source movie. Value conversions follow that version: in
  Flash 4, comparisons produce `1`/`0` and `true` prints as `1`; up to Flash 6, `undefined` converts to
  `""`/`0`, and a string is true only if it converts to a non-zero number.
- `#operators flash4|flash5` (only when it differs from the version's default) — which operations the
  plain operators mean. **flash4**: `+`, `==`, `<` are numeric (operands converted to numbers).
  **flash5**: they are the typed Flash 5 operators (`+` concatenates if either side is a string;
  `==` is ECMAScript loose equality; `<` compares strings when both are strings).

## Operators

| Operator | Meaning |
| --- | --- |
| `+ == <` | see `#operators` |
| `a != b`, `a >= b` | exactly `!(a == b)`, `!(a < b)` |
| `a > b`, `a <= b` | `b < a` and `!(b < a)`, with `a` evaluated first |
| `add`, `eq`, `ne`, `lt`, `ge`, `gt`, `le` | string concatenation and string comparisons (Flash 4 style) |
| `and`, `or` | Flash 4 logical operators: both sides always evaluated |
| `&&`, `\|\|`, `!` | short-circuit / logical not |
| `- * / %` | numeric. In Flash 4 movies, dividing by zero gives `"#ERROR#"` |
| `x++`, `x--` | statement shorthands for `x = Number(x) + 1` and `x = x - 1` |
| `__add4(a, b)`, `__add2`, `__eq4`, `__eq2`, `__lt4`, `__lt2`, `__gt2` | the other generation's operator, when a script mixes them |

## Variables and targets

- `name` reads a variable: locals, then the current timeline, then globals. `this`, `_root`,
  `_parent`, `_levelN` and `_global` are special.
- `eval("/holder:Score")`, `eval("../:x")`, `set("/:x", v)` access variables by path (slash syntax
  `path:variable`, or dot syntax). Assigning `name = v` without a path sets it on the current timeline
  unless a local exists.
- `tellTarget (path) { ... }` runs the body against another timeline: timeline actions **and** plain
  variables go to that clip. It is exactly `setTarget(path); ...; setTarget("");`. `setTarget` also exists
  on its own.
- Paths: `"/"` (level root), `"/a/b"`, `"../c"`, `"_level1/x"`, `"_root.a.b"`, or a clip value.

## Built-in actions

Timeline: `play()`, `stop()`, `nextFrame()`, `prevFrame()`, `gotoAndPlay(f)`, `gotoAndStop(f)` where `f`
is a 1-based number, a label, a numeric string, or `"path:frame"`; `gotoLabel("x")` is `gotoAndStop` for a label that looks like a number (and `gotoFrame(n)`
is an older spelling of `gotoAndStop(n)`); `call(f)` runs another frame's scripts; `frameLoaded(f)`.

Clips: `getProperty(target, _x)`, `setProperty(target, _alpha, 50)` (properties: `_x _y _xscale _yscale
_currentframe _totalframes _alpha _visible _width _height _rotation _target _framesloaded _name
_droptarget _url _highquality _focusrect _soundbuftime _quality _xmouse _ymouse`),
`duplicateMovieClip(src, name, depth)`, `removeMovieClip(t)`, `startDrag(t, lockCenter[, l, t, r, b])`,
`stopDrag()`.

Loading: `loadMovie(url, target)`, `loadVariables(url, target)`, `getURL(url, window)` (`"FSCommand:x"`
URLs are fscommands; a `_levelN` window loads a movie into that level).

Values: `random(n)`, `getTimer()`, `int(x)`, `length(s)`, `substring(s, start1Based, count)`, `ord(c)`,
`chr(n)` (Windows-1252 before Flash 6), `mbchr(n)`, `mblength(s)`, `mbord(c)`, `mbsubstring(s, start1Based, count)`
(Flash 4's multi-byte versions: the same here, as the text is single-byte), `targetPath(clip)`, `Number(x)`,
`String(x)`, `trace(x)` (to the game's log), `stopAllSounds()`, `toggleHighQuality()` (does nothing: the engine always
draws at full quality).

Anything else called by name is a user function or one of the globals below.

## Objects and methods (Flash 5)

The Flash 5 standard objects the game's scripts use, with their Flash behavior.

**Movie clips** (`_root`, `this`, `_parent`, any clip by path or value): `play()`, `stop()`, `nextFrame()`,
`prevFrame()`, `gotoAndPlay(f)`, `gotoAndStop(f)`; `attachMovie(linkageName, newName, depth)` (a symbol exported
under that name in the clip's movie: the settings dialogs are attached this way), `duplicateMovieClip(newName,
depth)`, `createEmptyMovieClip(name, depth)`, `removeMovieClip()`; `loadMovie(url)`, `unloadMovie()` (loads
`blank.swf` there); `swapDepths(depthOrClip)`; `hitTest(x, y[, shapeFlag])` (with `shapeFlag`, the clip's drawn shapes,
else its bounds) or `hitTest(otherClip)` (bounds overlap); `getBounds(space)` (`{xMin, xMax, yMin, yMax}` in that
clip's coordinates); `localToGlobal(point)`, `globalToLocal(point)` (change `point.x`/`point.y` in place);
`startDrag(lockCenter[, left, top, right, bottom])`, `stopDrag()`; `getBytesLoaded()`, `getBytesTotal()` (both 1:
everything is always loaded). Depths given to scripts are as Flash's (0 and up); the engine adds Flash's offset.

**Object**: `new Object()`, `hasOwnProperty(name)`, `toString()` (`"[object Object]"`), `valueOf()`.
**Function**: `call(thisObject, ...)`, `apply(thisObject, argsArray)`.

**Array**: `new Array(...)` (`new Array(n)`: n empty items), `length`, `push`, `pop`, `shift`, `unshift`, `join(sep)`,
`toString()`, `reverse()`, `concat(...)`, `slice(start, end)`, `splice(start, count, ...items)`; negative indices count
from the end, as in Flash.

**String**: `length`, `charAt(i)`, `charCodeAt(i)`, `indexOf(s[, from])`, `lastIndexOf(s)`, `substr(start, count)`,
`substring(start, end)`, `slice(start, end)`, `split(sep)`, `concat(...)`, `toUpperCase()`, `toLowerCase()`,
`toString()`, `valueOf()`; `String.fromCharCode(...)`. (0-based, unlike the Flash 4 `substring` action.)

**Number**: `toString([radix])`, `valueOf()`; `Number.MAX_VALUE`, `MIN_VALUE`, `NaN`, `POSITIVE_INFINITY`,
`NEGATIVE_INFINITY`. **Boolean**: `toString()`.

**Math**: `abs`, `floor`, `ceil`, `round` (halves up, as Flash: `Math.round(-2.5)` is `-2`), `sqrt`, `pow`, `exp`,
`log`, `sin`, `cos`, `tan`, `asin`, `acos`, `atan`, `atan2(y, x)`, `min(...)`, `max(...)`, `random()`; `PI`, `E`,
`SQRT2`, `LN2`, `LN10`.

**Global functions**: `parseInt(s[, radix])`, `parseFloat(s)` (the number at the start of the string, as Flash),
`isNaN(x)`, `isFinite(x)`, `escape(s)`, `unescape(s)`; `NaN`, `Infinity`.

**Key**: `Key.isDown(code)` (a key held now: Flash key codes, e.g. 37 to 40 for the arrows), `Key.getCode()` and
`Key.getAscii()` (the last key pressed). **Mouse**: `Mouse.hide()`, `Mouse.show()` (the system cursor; the game's own
cursor is a clip). **Selection**: `Selection.setFocus(field)`, `Selection.getFocus()` (the focused text field's path,
or null), `Selection.setSelection(start, end)` (accepted, does nothing).

**SharedObject**: `SharedObject.getLocal(name)` (the save: an object whose `data` is kept between sessions, and whose
`flush()` writes it; one file per name in the save folder). **System**: `System.setClipboard(text)`.
**Stage**: `Stage.width`, `Stage.height` (the original 770 by 475), `Stage.displayState` (`"normal"` or
`"fullScreen"`; setting it switches).

## Engine additions

Not Flash: added for this port, used by its own scripts (the settings, the panning).

| Name | Does |
| --- | --- |
| `Stage.mnogViewLeft`, `Stage.mnogViewRight` | The part of the stage the window shows, in stage coordinates: wider than 0 to 770 in widescreen. |
| `Stage.mnogClampScenery(layers, rates[, vertical])` | Keeps a panning scene inside its artwork: `layers` its layer clips, `rates` how fast each moves. Called by the player's panning each frame. |
| `System.mnogMenuOpen()` | Whether a menu covers the game (the Help, Settings, save slots...), so the scene doesn't pan behind it. |
| `System.mnogVersion` | The project's version (`version_number.txt`), shown on the title screen. |
| `_url` (property 15) | A loaded clip's movie, as `"Name.swf"` (`blank.swf` when emptied): scenes test it to see which music or sounds are already playing. |

**fscommands.** `getURL("FSCommand:name", arg)` asks the engine to do something (the settings scripts call it through
their helper `_level0.__matanui.util.fscommand(name, arg)`):

| Command | Does |
| --- | --- |
| `settings` | Opens the settings dialog, or closes it if it (or one of its pages) is open. |
| `help`, `saveslots` (`"title"`/`"game"`), `audiomenu`, `warp` | Opens the Help, the save slots, the Audio menu, the warp menu (closing the settings dialog). |
| `music N`, `wallpaper N` | Opens the chronicle entry N's music or wallpaper window. |
| `gameend` | Shows the end-of-game prompt (restart, chronicle, title screen). |
| `fullscreen`, `widescreen`, `smooth` (0/1/2), `vectorsand`, `textboxalpha`, `loadscreen`, `debug` | Applies a setting (the settings scripts call these with the saved value). |
| `message text` | Shows a short message over the view. |
| `scenesounds` | Fills in the scene's music and sound variables a save lacks (a save from before they were kept). |
| `fadeout LEVEL` | Fades out the sounds started by the movies on that level (the title screen's music and waves). |
| `quit` | Closes the game. |

## Statements

`if/else`, `while`, `do ... while`, `for ($r0 in obj)`, `break`, `continue`, `return`, `var`,
`function name(a, b) { }`, function expressions, object/array literals, `new`, and `goto L;` / `L:` labels
for jumps the decompiler could not express structurally. `$rN` are registers (`$r128` and up are
temporaries the decompiler introduces to preserve evaluation order).

Function, method and constructor arguments are evaluated **right to left**, as in Flash 5; built-in
action arguments left to right.
