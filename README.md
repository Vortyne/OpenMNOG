# OpenMNOG

A native, Flash-free version of the *Mata Nui Online Game* (LEGO / Templar Studios, 2001), with widescreen,
smooth motion and other enhancements.

The original SWF files were converted once into an open, readable format: vector shapes as SVG path data, sounds
as Ogg Vorbis, scripts as readable ActionScript-style source. A new engine (C# / .NET, SkiaSharp, SDL2) plays that
format. Nothing from Adobe is used. The game was released as freeware, so its converted content is part of this
repository in `game/`. That folder is now the game's source: this project's fixes and enhancements are edits to
it, never a reconversion. The converter that first made it is kept outside this repository.

## Running

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/Mnog.Engine -c Release -- game
```

`bash play.sh` builds the engine and opens the game on your save, checking the build and the game files first.
The game opens fullscreen, or in a window if it was last left that way; `--windowed` starts it in a window.

## What's new

- **Widescreen**: the view fills a wide screen with the scenery beyond the original stage edges, as far as each
  scene's artwork reaches (skies and some backdrops are extended to fit).
- **Smoothing**: drawing runs at the display's refresh rate. Panning and fades are smoothed between the game's
  18 fps frames; with Full (or Partial, in scenes marked for it) animation is too.
- **Vectorized pictures**: the photographic sand, sky, Mt. Ihu and other pictures are redrawn as vector art that
  stays sharp at any size. Skyboxes were upscaled instead, to keep their aesthetic.
- **Title screen**: the first beach view with the BIONICLE logo and the Beach Chant, with Continue, New Game,
  Exit, Settings and Save Slot.
- **Save slots**: 12 slots, each with a picture of where it was last saved. The game saves by itself.
- **Chronicle music**: an entry's music button plays its soundtrack in full quality (FLAC, in
  `game/audio/soundtrack`; `soundtrack.json` says which entries have which tracks), and Download opens that folder
  with the track selected (a note there asks players to copy the files rather than move or delete them).
  Entries without music yet have the button greyed out.
- **In-game Help**: the original support site's General, Tips and Hints and Bionicle Decoder pages, rebuilt inside
  the game (pictures are live frames of the game itself), plus the new keyboard controls.
- **Settings**, grouped into Language, Graphics, Gameplay and Audio, each option described when the mouse is over
  it: fullscreen, widescreen, smoothing, vectorized pictures, loading screen, textbox transparency, dialog speed,
  panning speed and edge zone, title screen, and volume for master, music and sound effects.
- **Skip** button for cutscenes
- fixes for small errors in the original artwork.
- **High quality audio** (Audio menu, off by default): sounds and music play from high-quality originals where
  they've been found. The files are in `game/audio/hq` (WAV, so they load at once), and `game/audio/hq/hq.json` says
  which of the game's sounds each replaces. Separately, a WAV or FLAC beside a sound's Ogg (`sounds/NAME.wav`) is
  always played in its place (the title music).

## Controls

The original mouse controls all work as before. New keys:

| Key | Does |
| --- | --- |
| Arrow keys, W A S D | Pan the view |
| Left Shift | Turn around |
| Space | Continue a conversation |
| Esc | Close the open window, or ask to quit |
| F11, Alt + Enter | Toggle fullscreen |
| < / > | In a movie: go back / forward one frame (pauses it) |
| / or Pause | In a movie: pause or play on |
| V | Toggle vectorized pictures (to compare with the original art) |
| =, Backspace (with Settings open) | Warp menu (every scene, cutscene and minigame) / debug panel |

## Save data

Settings and progress are saved in `Documents\My Games\OpenMNOG\sharedobjects` (a Flash-style shared object, as
the Project Brick patch used). Slot 1 is the original save. Slot pictures (`pictures/`) and backups of the save
file (`backups/`) are kept in `Documents\My Games\OpenMNOG`. A save from before it moved there (in
`%APPDATA%\MataNuiOnlineGame`) is copied over the first time the game opens.

To keep them somewhere else, set `Folder` in `OpenMNOG.ini`, beside the `game` folder: a full path, one with
environment variables (`%USERPROFILE%\Saves`), or one relative to the ini (`saves`, to keep them with the game).

## Reporting bugs and sounds

The Help's **Report a bug** page links to both forms. [Report a bug](https://github.com/Vortyne/OpenMNOG/issues/new?template=bug_report.yml)
in the game, or, if you recognise one of the game's sounds, [report where it comes from](https://github.com/Vortyne/OpenMNOG/issues/new?template=sound_found.yml)
so it can be replaced with the high-quality original. The [sound list](https://vortyne.github.io/OpenMNOG/sounds/) plays
every sound still without a high-quality source in the browser. The [project page](https://vortyne.github.io/OpenMNOG/) (`site/`, published by `.github/workflows/site.yml`) offers the download for the version in `version_number.txt`.

## Documentation

- [docs/FORMAT.md](docs/FORMAT.md): the game's files: movies, shapes, timelines and sounds as JSON, and the files
  beside a movie that add the port's enhancements (widescreen stretches, pan limits, vector pictures...).
- [docs/SCRIPT.md](docs/SCRIPT.md): the scripts (`.as`): the language, the built-in actions, the Flash objects and
  methods, and the engine's additions and fscommands.
- [README-release.txt](README-release.txt): the readme in each release download (running the game on each system,
  where saves go, reporting a bug).
- `game/files/menu-text.json`: the menus' German and French text (see its note).

## Repository

| Path | Contents |
| --- | --- |
| `game/` | The game: scenes, scripts, artwork, sounds and the Help's pages ([docs/FORMAT.md](docs/FORMAT.md)). Edit it directly. |
| `src/Mnog.Engine` | The engine: Skia rendering (GPU via OpenGL, CPU fallback), SDL2 window, input and audio, the script interpreter. |
| `src/Mnog.Format` | The game format's JSON documents ([docs/FORMAT.md](docs/FORMAT.md)). |
| `src/Mnog.Script` | The script language: syntax tree, parser, printer ([docs/SCRIPT.md](docs/SCRIPT.md)). |
| `assets/` | The original sources of the enhancements, kept for reference: the menus' font (TradeMarker Light), the title screen's logo, music and sand, artwork fixes, vector and upscaled pictures. |
| `version_number.txt` | The version, shown on the title screen as "OpenMNOG v" followed by it. |

## Development

**Test mode.** `--run "<commands>"` plays the game headlessly (coordinates are stage pixels), for example:

```bash
dotnet run --project src/Mnog.Engine -- game --run "wait 40; click 690 255; wait 60; shot out.png"
```

| Command | Does |
| --- | --- |
| `wait N` | Run N frames |
| `move X Y`, `click X Y`, `key CODE [BUTTON]`, `type TEXT` | Mouse and keyboard input (BUTTON: the code buttons' keyPress events see, e.g. `key 8 8` for Backspace) |
| `shot FILE [W H [T]]` | Screenshot (T: partway between game frames) |
| `scene NAME` | Go to a scene, as the warp menu does |
| `set VAR VALUE`, `get PATH` | Variables, relative to `_level1` (e.g. `get /Backpack/Backpack/:Slot1`) |
| `load URL TARGET` | loadMovie (e.g. `load blank.swf _level1/Music-holder`) |
| `widescreen 0\|1`, `smooth 0\|1`, `fs COMMAND ARG` | Settings for the run (`fs` as the game's scripts call them, e.g. `fs vectorsand 0`) |
| `vimg MOVIE ID SCALE FILE` | An image's vector replacement alone, as the engine draws it |
| `art X Y` | The shapes drawn at a stage point |
| `playframes FILE W H [FROM TO]` | The scene played through, every frame appended to FILE as raw RGB (frame numbers in FILE.idx) |
| `freeze MS` | Stall the game (tests the freeze watchdog, with `MNOG_WATCHDOG=1`) |
| `sounds`, `tree [depth]`, `pos PATH...` | Playing sounds, the display list, clip positions |
| `bench N W H`, `profile N W H` | Frame timings |
| `cache 0\|1`, `cachestats`, `drawprof`, `extentms` | Rendering diagnostics |

`--run @file` reads the commands from a file, and `--movie NAME` starts a single movie instead of the game.

**Environment variables.** `MNOG_SAVEDIR` uses another save folder (test runs use a copy, so your save is never
touched). `MNOG_NOVSYNC=1` draws as fast as possible. `MNOG_NOCACHE=1` turns off the layer cache: artwork that
isn't changing is drawn from cached images (`src/Mnog.Engine/Render/LayerCache.cs`), so panning slides them
instead of redrawing every shape. `MNOG_WATCHDOG=1` turns the freeze watchdog on in test runs (it's always on in
the game). `MNOG_VECDBG=1` logs each vector picture as it's drawn, and `MNOG_PLAYT` makes `playframes` capture
partway between game frames (0..1), as Smoothing draws them.

**Logs.** `play.sh` writes the game's log to `build/game-run.log` (frame timings for each scene, slow frames). A
crash writes its details to `logs/crash-<time>.log`, and a freeze (no frame for 3 seconds) writes what the game
was doing and waiting for to `logs/hang-<time>.log` (`logs/` is next to `game/`, beside the game's exe in a release).

**Sounds.** `game/sound-groups.json` lists which cutscene and minigame sounds follow the Music slider and which
the SFX slider; other sounds follow the engine's rules (the `Music_*` movies are music).

## Was AI used?

| | |
| --- | :---: |
| Generating art | ❌ |
| Generating music | ❌ |
| Writing code | ✅ |

## Credits

The *Mata Nui Online Game* is by Templar Studios for LEGO. The title music is *Beach Chant (HQ Remake by
Essenger)*.

The French translation is by ARROW_404.
