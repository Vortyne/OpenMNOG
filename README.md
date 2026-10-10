# Mata Nui Online Game — native

A native, Flash-free version of the *Mata Nui Online Game* (LEGO / Templar Studios, 2001). The original
SWF files are converted once into an open, readable format — vector shapes as SVG path data, sounds as
Ogg Vorbis, scripts as readable ActionScript-style source — and a new engine plays that format.

Nothing from Adobe is used. The game was released as freeware, so its converted content is part of this
repository, in `game/`: that folder is the game's source now, readable and editable (scenes, scripts, artwork,
sounds), and this project's fixes and enhancements are edits to it. The converter that first made it from the
original SWF files is maintained separately, outside this repository.

## Layout

| Project | Purpose |
| --- | --- |
| `src/Mnog.Format` | The converted format's JSON documents ([docs/FORMAT.md](docs/FORMAT.md)). |
| `src/Mnog.Script` | The script language: syntax tree, parser, printer ([docs/SCRIPT.md](docs/SCRIPT.md)). |
| `game/` | The game itself, converted and with this project's fixes ([docs/FORMAT.md](docs/FORMAT.md)). Edit it directly. |
| `src/Mnog.Engine` | The game engine: Skia rendering (GPU via OpenGL, CPU fallback), SDL2 window/input/audio, script interpreter. |
| `assets/` | Sources the game was enhanced with: fonts, title screen logo and music, artwork fixes, vector sand, upscaled images. |
| `version_number.txt` | The project's version, shown on the title screen as "OpenMNOG v" followed by it. |

## Building and running

Requirements: .NET 10 SDK.

```bash
dotnet run --project src/Mnog.Engine -c Release -- game
```

The engine also finds `game/` next to (or above) its executable on its own. `bash play.sh` builds the engine and
opens the game on your own save, checking the build and game files first.

Controls are the original mouse controls. The in-game **Settings** menu (key icon) has a **Widescreen**
option: the view fills the window's width with the scenery beyond the original stage edges, as far as
each scene's artwork reaches. **Smooth** draws at the display's refresh rate: scene panning and
transition fades are interpolated between game frames, while animations and movies keep their original
frame rate (18 fps). Both are on by default. The Settings menu also has **Language** (switches without reloading; the current one is highlighted) and
**Misc** (Timestamp, Progress) submenus and **Dialog speed** (Normal, the original typing speed; Fast, 2x;
Instant; the question box and a conversation's opening slide-up move 2x as fast on Fast and at once on Instant; with Smooth on, the typed text appears letter by letter between game frames, each letter fading in), **Title screen** and **Loading screen** (Off goes from scene to scene directly, without the fade to black and the globe, with a 200 ms crossfade). **Space** continues a dialog, as clicking its red arrow does. The game always runs at its original 18 fps: Project Brick's 30 fps option was removed because it
breaks the timing of many scenes. During cutscenes a **Skip** button
on the bottom bar jumps to where the cutscene hands back to the game. Pressing **W** while the Settings menu is
open opens a warp menu listing every scene, cutscene and minigame (type to filter, Enter or click to go there).
Pressing **D** while the Settings menu is open toggles a debug panel
over the bottom bar (scene, frame, fps, mouse/button under it, music and sound movies, playing
sounds, latest warning). Its **Report edge issue** button (or F8) records the current view as a widescreen edge problem: a line in `edge-reports/reports.txt` (scene, layer positions, view, measured edges, pan limits) and a screenshot beside it. The game opens fullscreen, or windowed if it was last left that way (`--windowed` starts it in a window), on a **title screen**: the first beach view with the
BIONICLE logo, the Beach Chant and the waves, and **Continue** / **New Game** / **Exit**, with **Settings** (the
settings menu, as in game) and **Save Slot** in the bottom right and the version in the bottom left. New Game fades the
title away into the game on that same view; Continue goes straight to loading the saved game; New Game over a save asks first, with the settings menu's Start New Game? dialog. Settings has
**Title screen: On/Off**. **Esc** asks "Quit game?" (or closes an open dialog). **F11**, **Alt+Enter** or the bar's
fullscreen button toggle fullscreen. Saved settings and
progress (the Project Brick patch uses a Flash "shared object") live in
`%APPDATA%\MataNuiOnlineGame\sharedobjects`.

### Engine test mode

`--run "<commands>"` plays headlessly: `wait N` (frames), `move X Y`, `click X Y`, `key CODE`,
`shot file.png [W H [T]]` (T: draw partway between game frames), `bench N W H`, `widescreen 0|1`, `smooth 0|1` (the game's saved setting is applied at startup and overrides these), `type TEXT` (key presses),
`load URL TARGET` (loadMovie, e.g. `load blank.swf _level1/Music-holder`), `sounds` (list playing sounds), `tree [depth]` (dump the display list),
`scene NAME` (go to a scene as the warp menu does, replaying the variables a navigation button leading there sets), `set VAR VALUE` / `get PATH` (variables, relative to
`_level1`, e.g. `get /Backpack/Backpack/:Slot1`), `pos PATH...` (clip x positions), `profile N W H` (run and draw N frames, reporting slow ones),
`extentms` (widescreen edge-measurement timings; with `MNOG_EXTENTDBG=1` short edges and clamp decisions are logged), `drawprof` (per-shape draw
times, with `MNOG_DRAWPROF=1`). `--run @file` reads the commands from a file. `MNOG_SAVEDIR=<folder>` uses another save folder (test runs use a copy so the player's save is never touched).
`--movie Name` starts a single movie instead of the game, to test a scene in isolation. Coordinates are stage pixels. Example:

```bash
dotnet run --project src/Mnog.Engine -- game --run "wait 40; click 690 255; wait 60; shot out.png"
```

## Title screen assets

`assets/title` holds the title screen's sources, used by the converter's title patch: `bionicle_logo.json`, the BIONICLE logo vectorized from `BIONICLE_Logo.png`
(by a helper script kept outside this repository), and the
music, *Beach Chant (HQ Remake by Essenger)*, encoded to Ogg Vorbis with ffmpeg during conversion.
Fixes to flaws in the original artwork live in `assets/fixes/MOVIE.json` (shapes added to a sprite at a
given depth, in its own coordinates; applied by the converter after conversion), e.g.
`IX.1-W.json` fills two gaps in the desert's sand that showed the background through. A fix can also place another
(e.g. mirrored) copy of a symbol, remove a stray piece the original kept just off the stage (`remove`; a leaf in V.2-S),
stretch a placed backdrop horizontally while widescreen is on
(`widescreenScaleX`; the top band of IV.5-N and XIV.8-E), and `"pan": {"min", "max"}` narrows a scene's widescreen pan range by hand where
the measured one goes further than the artwork looks right (IV.5-N). Other widescreen patches widen the skies
(continued as their mirror image) and the flute's backdrop.

**Save slots**: 12 slots (4 to a tab) with a picture of where each was last saved. The bottom bar's Change Save Slot
button (formerly Start New Game) opens them in game: picking a slot loads it (an empty one starts a new game). The
title screen's Save Slot button picks the slot Continue and New Game use (New Game over a save asks first). Saves can
be deleted, after a confirmation; in game, the slot being played has Restart instead (the original's Start New Game,
also confirmed). The title screen's Save Slot button (bottom right) shows the selected slot in a box joined below it. The game saves by itself, so there's no save or load. Slot 1 is the original save;
pictures are kept in `pictures/` beside the save data. The menus' font, TradeMarker Light, is in `assets/fonts`
(the converter takes the glyphs the original lacks from it).

If the game crashes, the error is saved to `build/crash-<time>.log`.

Rendering: artwork that isn't changing is drawn from cached images (`src/Mnog.Engine/Render/LayerCache.cs`), so
panning a detailed scene only slides those images instead of redrawing every vector shape; animated parts are
drawn as usual in between. `MNOG_NOCACHE=1` turns it off (harness: `cache 0|1`, `cachestats` with `MNOG_CACHESTATS=1`).

`sand_extension.png` fills the strip below the beach (where the game's menu bar is) on the title screen: it was
painted onto a 3840x2160 render of the 16:9 view and covers its rows 1937-2159.
