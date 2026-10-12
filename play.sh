#!/bin/bash
# play.sh: the one way to open the game for playing. Builds the Release engine, checks the converted data is
# in place, then opens the game on the real save. Refuses (and says why) instead of opening anything wrong.
cd "$(dirname "$0")"
SAVE='//localhost/c$/Users/Michael/AppData/Roaming/MataNuiOnlineGame/sharedobjects'   # forward slashes: Git Bash mangles backslashes
DLL=src/Mnog.Engine/bin/Release/net10.0/Mnog.Engine.dll
LOG=build/game-run.log

fail() { echo "NOT LAUNCHED: $*"; exit 1; }

running() { powershell -NoProfile -c "@(Get-CimInstance Win32_Process -Filter \"Name='dotnet.exe'\" | ? { \$_.CommandLine -like '*$1*' }).Count"; }

# One game at a time, and never while the data is being converted.
[ "$(running Mnog.Engine.dll)" = "0" ] || fail "the game is already running; close it first"
[ "$(running Mnog.Converter)" = "0" ] || fail "a conversion is running; wait for it to finish"

# The game: game/ (converted from the original once, then edited directly; never reconverted to apply a change).
[ -f game/game.json ] || fail "game/ has no game.json"

# Engine: build Release and make sure the DLL is current.
out=$(dotnet build src/Mnog.Engine -c Release 2>&1)
echo "$out" | grep -q "Build succeeded" || { echo "$out" | grep -E "error|locked" | head; fail "engine build failed"; }
echo "$out" | grep -qi "locked" && fail "engine DLL was locked during the build"
stale=$(find src/Mnog.Engine -name '*.cs' -not -path '*/obj/*' -newer "$DLL" | head -3)
[ -z "$stale" ] || fail "engine DLL is older than $stale"

[ -d "$SAVE" ] || fail "real save folder not found: $SAVE"

# (Extra arguments go to the game, e.g. --autopilot "scene Episode_9-1; wait 40; goto holder 2205 play" to open it there.)
MNOG_SAVEDIR="$SAVE" dotnet "$DLL" game "$@" > "$LOG" 2>&1 &
for _ in $(seq 1 20); do grep -q "startup:" "$LOG" 2>/dev/null && break; sleep 0.5; done
first=$(head -1 "$LOG")
case "$first" in
  *"save: \\\\localhost\\c\$\\Users\\Michael\\AppData\\Roaming\\MataNuiOnlineGame\\sharedobjects"*) ;;
  *) fail "wrong save path in log: $first" ;;
esac
grep "loaded\|startup" "$LOG"
echo "LAUNCHED (engine $(date -r "$DLL" +%H:%M:%S), save OK)"
wait
