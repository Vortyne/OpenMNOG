# MNOG script (`.as`)

The converter decompiles every piece of Flash bytecode into a small ActionScript 1 dialect. It reads like
the original source, and every construct has exactly the behavior of the bytecode it came from, so
scripts can be read, edited and recompiled without changing the game.

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
`chr(n)` (Windows-1252 before Flash 6), `mb*` variants, `targetPath(clip)`, `Number(x)`, `String(x)`,
`trace(x)`, `stopAllSounds()`, `toggleHighQuality()`.

Anything else called by name is a user function or a global (`Math`, `parseInt`, `SharedObject`, ...).

## Statements

`if/else`, `while`, `do ... while`, `for ($r0 in obj)`, `break`, `continue`, `return`, `var`,
`function name(a, b) { }`, function expressions, object/array literals, `new`, and `goto L;` / `L:` labels
for jumps the decompiler could not express structurally. `$rN` are registers (`$r128` and up are
temporaries the decompiler introduces to preserve evaluation order).

Function, method and constructor arguments are evaluated **right to left**, as in Flash 5; built-in
action arguments left to right.
