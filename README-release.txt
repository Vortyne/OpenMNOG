OpenMNOG
========

A native, enhanced version of the Mata Nui Online Game (BIONICLE, 2001): the original game with widescreen,
smooth animation, sharp redrawn artwork, high-quality sound, more save slots and new settings. It runs on its own:
no Flash, browser or other download is needed.

Project page and newer versions: https://github.com/Vortyne/OpenMNOG


Running the game
----------------

Unpack the whole folder first (don't run the game from inside the zip), anywhere you like, and keep its files
together: the program needs the game/ folder and the files beside it.

Windows
  Double-click OpenMNOG.exe.
  If Windows shows "Windows protected your PC", click "More info", then "Run anyway". (The game isn't signed with a
  paid certificate, so Windows doesn't recognise it yet.)

macOS
  macOS blocks programs downloaded from outside the App Store until they're approved. Once, after unpacking, open
  Terminal, go to the game's folder (type "cd ", drag the folder into the Terminal window, press Return) and run:
      xattr -dr com.apple.quarantine .
      codesign --force --deep -s - OpenMNOG
  Then start the game with:
      ./OpenMNOG
  (Download the macos-arm64 version for Macs with Apple silicon, M1 and later; macos-x64 for Intel Macs.)

Linux
  Unpack with: tar -xzf OpenMNOG-*-linux64.tar.gz
  Then, in the game's folder, run: ./OpenMNOG


Playing
-------

Everything works with the mouse, as in the original. Some keys are new:
  Arrow keys or W A S D   look around
  Left Shift              turn around
  Space                   continue a conversation
  Esc                     close the open window, or ask to quit
  F11 or Alt + Enter      switch between full screen and a window
The in-game Help (the ? button on the bar) explains the game and lists every key.

Settings (the key button on the bar, or Settings on the title screen) choose the language (English, German or
French), the graphics, the gameplay options and the volume.


Your save
---------

The game saves by itself. Saves are kept in:
  Windows         Documents\My Games\OpenMNOG
  macOS, Linux    ~/.config/OpenMNOG
To keep them somewhere else (beside the game, for example, to carry it on a USB stick), see OpenMNOG.ini in this
folder.


If something goes wrong
-----------------------

If the game crashes or stops responding, it writes what happened to a file in the logs folder, next to this one.
Please report it on the project page (Issues), with that file attached:
  https://github.com/Vortyne/OpenMNOG/issues
In the game, Help > Report a bug opens the form too.


The Mata Nui Online Game is by Templar Studios for LEGO. OpenMNOG is a fan project, not made or endorsed by LEGO.
