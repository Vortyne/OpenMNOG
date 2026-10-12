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
- **Vectorized pictures**: the photographic sand, sky and other pictures are redrawn as vector art that stays sharp
  at any size. Some images are upscaled instead.
- **Title screen**: the first beach view with the BIONICLE logo and the Beach Chant, with Continue, New Game,
  Exit, Settings and Save Slot.
- **Save slots**: 12 slots, each with a picture of where it was last saved. The game saves by itself.
- **In-game Help**: the original support site's General, Tips and Hints and Bionicle Decoder pages, rebuilt inside
  the game (pictures are live frames of the game itself), plus the new keyboard controls.
- **Settings**, grouped into Language, Graphics, Gameplay and Audio, each option described when the mouse is over
  it: fullscreen, widescreen, smoothing, vectorized pictures, loading screen, dialog speed, panning speed and edge
  zone, title screen, and volume for master, music and sound effects.
- **Skip** button for cutscenes, and fixes for flaws in the original artwork.
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
| W, D (with Settings open) | Warp menu (every scene, cutscene and minigame) / debug panel |

## Save data

Settings and progress are saved in `%APPDATA%\MataNuiOnlineGame\sharedobjects` (a Flash-style shared object, as
the Project Brick patch used). Slot 1 is the original save. Slot pictures (`pictures/`) and backups of the save
file (`backups/`) are kept in `%APPDATA%\MataNuiOnlineGame`.

## Reporting bugs and sounds

The Help's **Report a bug** page links to both forms. [Report a bug](https://github.com/Vortyne/OpenMNOG/issues/new?template=bug_report.yml)
in the game, or, if you recognise one of the game's sounds, [report where it comes from](https://github.com/Vortyne/OpenMNOG/issues/new?template=sound_found.yml)
so it can be replaced with the high-quality original. The [sound list](https://vortyne.github.io/OpenMNOG/sounds/) plays
every sound still without a high-quality source in the browser. The [project page](https://vortyne.github.io/OpenMNOG/) (`site/`, published by `.github/workflows/site.yml`) offers the download for the version in `version_number.txt`.

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
| `move X Y`, `click X Y`, `key CODE`, `type TEXT` | Mouse and keyboard input |
| `shot FILE [W H [T]]` | Screenshot (T: partway between game frames) |
| `scene NAME` | Go to a scene, as the warp menu does |
| `set VAR VALUE`, `get PATH` | Variables, relative to `_level1` (e.g. `get /Backpack/Backpack/:Slot1`) |
| `load URL TARGET` | loadMovie (e.g. `load blank.swf _level1/Music-holder`) |
| `widescreen 0\|1`, `smooth 0\|1` | Settings for the run |
| `sounds`, `tree [depth]`, `pos PATH...` | Playing sounds, the display list, clip positions |
| `bench N W H`, `profile N W H` | Frame timings |
| `cache 0\|1`, `cachestats`, `drawprof`, `extentms` | Rendering diagnostics |

`--run @file` reads the commands from a file, and `--movie NAME` starts a single movie instead of the game.

**Environment variables.** `MNOG_SAVEDIR` uses another save folder (test runs use a copy, so your save is never
touched). `MNOG_NOVSYNC=1` draws as fast as possible. `MNOG_NOCACHE=1` turns off the layer cache: artwork that
isn't changing is drawn from cached images (`src/Mnog.Engine/Render/LayerCache.cs`), so panning slides them
instead of redrawing every shape.

**Logs.** `play.sh` writes the game's log to `build/game-run.log` (frame timings for each scene, slow frames). A
crash writes its details to `build/crash-<time>.log`. The debug panel's **Report edge issue** button (or F8)
records a widescreen edge problem in `edge-reports/`.

**Sounds.** `game/sound-groups.json` lists which cutscene and minigame sounds follow the Music slider and which
the SFX slider; other sounds follow the engine's rules (the `Music_*` movies are music).

## Credits

The *Mata Nui Online Game* is by Templar Studios for LEGO. The title music is *Beach Chant (HQ Remake by
Essenger)*.
