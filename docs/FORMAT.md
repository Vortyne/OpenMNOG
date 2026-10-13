# Converted game format (`mnog-game/1`)

A converted game is a plain folder:

```
game.json                 manifest: title, entry movie, stage size, frame rate
movies/<Name>/movie.json  one folder per original SWF (Name = file name without .swf)
movies/<Name>/scripts/    *.as scripts (see SCRIPT.md)
movies/<Name>/images/     <id>.jpg (original JPEG bytes) or <id>.png (JPEG + alpha)
movies/<Name>/sounds/     <id>.ogg (Ogg Vorbis, trimmed to the exact samples Flash plays)
movies/<Name>/...         enhancement files (widescreen, vector pictures, ...: see the end of this document)
files/                    data files scripts load (loadVariables text, the Help's pages, images)
conversion-report.txt     anything the converter could not convert exactly
```

Scripts refer to other movies by their original names (`loadMovie("I.1-S.swf", ...)`); the engine maps
`X.swf` to `movies/X/` case-insensitively. Other URLs resolve under `files/`.

## Conventions

- Coordinates are **pixels** (Flash twips ÷ 20), y pointing down.
- Matrices are `[a, b, c, d, tx, ty]`: `x' = a·x + c·y + tx`, `y' = b·x + d·y + ty`.
- Colors are `"#RRGGBBAA"`.
- Color transforms are `[rMul, gMul, bMul, aMul, rAdd, gAdd, bAdd, aAdd]` with adds in 0..255:
  `c' = c·mul + add`, applied to unpremultiplied colors.
- Frame numbers are **1-based**, as in ActionScript.
- Optional fields are omitted when they have their default value.

## movie.json

```json
{
  "format": "mnog-movie/1",
  "source": "I.1-N.swf",
  "swfVersion": 4,
  "width": 770, "height": 425, "frameRate": 18,
  "background": "#FFFFFFFF",
  "symbols": [ ... ],
  "timeline": { ... },
  "exports": { "linkageName": symbolId }
}
```

`swfVersion` matters: value conversions in scripts follow the original Flash version's rules.

### Symbols

Every symbol has an `id` and a `kind`.

**shape** — `bounds` `[xMin, yMin, xMax, yMax]` and `parts`, drawn in order. Each part is a `path`
(SVG path data using only `M`, `L`, `Q`, `Z`) with either a `fill` (filled with the **even-odd** rule) or a
`stroke`.

- `fill`: `{"type": "solid", "color"}`, or a gradient `{"type": "linear" | "radial" | "focal", "matrix",
  "stops": [{"at": 0..1, "color"}], "spread"?, "linearRgb"?, "focal"?}` where the matrix maps the unit
  gradient square (−1..1; linear runs along x, radial has radius 1) into shape space, or a bitmap
  `{"type": "bitmap", "bitmap": imageId, "matrix", "repeat", "smooth"}` where the matrix maps image pixels
  into shape space.
- `stroke`: `{"width", "color" | "fill", "cap"?, "join"?, "miterLimit"?}`; caps and joins default to
  round. Flash draws strokes at least one device pixel wide.

Where two of a shape's opaque fills share an edge, the engine draws a hairline along it in the lower fill's paint,
so the antialiased edges leave no seam between them (Flash fills a shape's regions together); outer edges are
left as they are.

**morph** — a shape tween: each part has `start`/`end` paths with identical command sequences and
`fillStart`/`fillEnd` or `strokeStart`/`strokeEnd`. An instance's `ratio` (0..1) interpolates points,
colors, gradient matrices/stops and widths linearly.

**image** — `file`, `width`, `height`. Only used through bitmap fills.

**sprite** — a movie clip: its own `timeline`.

**button** — `records` (`states` ⊆ up/over/down/hit, `id`, `depth`, `matrix`?, `cxform`?), `actions`
(`on`: transition names — `rollOver`, `rollOut`, `press`, `release`, `releaseOutside`, `dragOver`,
`dragOut`, `menuDragOver`, `menuDragOut`, `keyPress` with `key` — and a `script`), `sounds` keyed by
`rollOut`/`rollOver`/`press`/`release`, `trackAsMenu`?. Button scripts run on the timeline containing
the button.

**font** — `name`, `bold`?, `italic`?, `unitsPerEm` (1024), `ascent`/`descent`/`leading`?, and `glyphs`:
`{"char", "path" (font units, even-odd), "advance"?}`.

**text** — static text: `bounds`, `matrix`, `runs` of `{font, size, color, x, y, glyphs (indices),
advances (px), text (informational)}`. Glyph `i` is drawn at the running x with scale `size / unitsPerEm`.

**edittext** — a dynamic/input field: `bounds`, `variable`?, `text`?, `font`?, `size`, `color`?, `align`,
margins, `indent`, `leading`, flags (`wordWrap`, `multiline`, `password`, `readOnly`, `selectable`,
`border`, `html`, `useOutlines`, `autoSize`). A field bound to `variable` shows that variable of its
parent clip (or a `path:name`).

**sound** — `file`, `rate`, `channels`, `samples`, `original` (codec in the SWF, informational).

### Timelines

```json
{ "frameCount": 59, "frames": [ { ... }, ... ], "stream": { "file", "startFrame", "rate", "channels" } }
```

Each frame may have:

- `labels`: frame labels.
- `ops`: display list changes in order. `{"remove": depth}` clears a depth. `{"place": depth, ...}`
  places or modifies: `id` (symbol), `move` (true = modify/replace the object already there), `matrix`,
  `cxform`, `ratio`, `name`, `clipDepth` (this object masks depths up to `clipDepth`), `events` (Flash 5
  clip events: `{"on": [...], "key"?, "script"}`).
- `scripts`: files run, in order, after the frame's display list is in place.
- `sounds`: `{"id", "stop"?, "noMultiple"?, "in"?, "out"? (44.1 kHz samples), "loops"?, "envelope"?:
  [[pos44, left 0..32768, right], ...]}`.

Going to a frame produces exactly the display list a sequential play would have reached; objects that
persist across the jump keep their identity (and any changes scripts made to them).

## Enhancement files

Files beside a movie's `movie.json` that add to or adjust it for this port. All are optional.

**`vector-images/<id>.json`** — a vector replacement for image `<id>`, drawn in its place with Vectorize bitmaps on
(drawn in the background at the size it's shown, the photo showing meanwhile). Coordinates are the image's pixels.

- `width`, `height`; `height` may be taller than the image, where the replacement continues below it.
- `parts`: shapes drawn in order, `{"path", "color"}` plus optionally `rule` ("nonzero"; even-odd by default),
  `stroke`/`cap`/`join`/`miter`, `blur`, `clip` (a path), and `gradient`: linear `{"from", "to", "stops":
  [[0..1, colour], ...]}`, radial `{"type": "radial", "center", "radius", "focal"?}` (either with an SVG-style
  `matrix`), or `{"type": "mesh", "cols", "rows", "step", "origin": [x, y], "data"}`: a grid of colours (base64
  RGBA, unpremultiplied, one every `step` pixels from `origin`) blended smoothly between them.
- `clip`: drawn only inside this outline; outside it the image shows.
- `hideImage`: the image itself isn't drawn under the replacement (its edge would show past a cleaner outline).
- `fade`: `[[row, opacity], ...]`, the replacement's strength down the image, the image showing through.
- `texture`: a repeating tile of shapes over it (`size`, `parts`, `blend`, `fade`); `layers`: more replacements
  over other parts of the image, each with its own `clip`, `parts` and `texture`.
- `seal`: each shape's edge drawn as a hairline in its own colour (flat tones meeting leave no seams).

**`vector-images/<id>.ref`** — one line naming another movie's replacement (`VII.10-S/vector-images/36.json`), for
a copy of the same picture in another movie: stored once. (`upscaled-images/<id>.ref` does the same for upscaled
pictures.)

**`upscaled-images/<id>.png`** — an upscaled copy of the image, always drawn in its place;
**`hires-images/<id>.png`** — a higher-resolution drawing, drawn in its place with Vectorize bitmaps on.

**`widescreen.json`** — placed objects stretched to fill a wide view: `[{"Sprite", "Depth", "Id"?, "ScaleX",
"ScaleY"?, ...}]`, the object at that depth of that sprite's timeline (0: the movie's main timeline), only symbol
`Id` if given. It's stretched from `Anchor` (a fraction of its width, 0.5 by default) or `AnchorX` (in its own
coordinates, not the stage's); `ScaleY` stretches it down from its top, or with `SplitY` only the part below that y.
`Split` / `SplitAt` (and `SplitLeft` / `SplitLeftAt`, with `ScaleLeft`) stretch only the part right (left) of a
point, leaving the rest as it is.

**`pan-limits.json`** — `{"Min", "Max"}`: the range the scene's panning keeps to (the Middle-Ground layer's
position), replacing the one measured from where its artwork ends; `null` keeps that end measured.

**`view.json`** — `{"Left", "Right", "Backdrop"}`: how far past the stage a scene shows, and a colour behind it.
**`widescreen.txt`** — a cutscene or minigame shown at the full widescreen width (otherwise kept to the stage);
**`stage-width.txt`** — a scene kept to the original stage width.

**`smooth-motion.txt`** — the scene's timeline animation smoothed with Smoothing on Partial: all of it (the file
there, e.g. holding just `1`), or with `a-b` lines those frames only; `!a-b` lines leave frames out (blending looks
wrong there), even on Full.

**`frame-rate.txt`** — a frame rate for the scene (e.g. `10`).

**`vector-moves.json`** — objects sitting on a vectorized picture moved to fit it while Vectorize bitmaps is on:
`{"Middle-Ground/#7": [dx, dy]}` (a path; `#N` is the child at depth N), or for a clip whose frames show different
things `{"1-24": [dx, dy], ...}` by its current frame; in the parent's coordinates.

**`backdrop.json`** — live backdrops: an image (`image`) replaced by a live copy of a scene (`scene`, `frame`,
`layers`, `zoom`/`x`/`y`, `blur`, `clip`, `overlay`, `if`, ...), as the chronicle's pictures are.
