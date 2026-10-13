#!/bin/bash
# release.sh: builds the release download, build/release/OpenMNOG-v{version}-win64.zip, to attach to the GitHub
# release tagged v{version} (the version from version_number.txt; the project page's download button links there).
#
# The zip holds one folder, OpenMNOG-v{version}-win64/, with:
#   OpenMNOG.exe (the engine, self-contained: players need no .NET), its native libraries (Skia, SDL),
#   game/ (the committed game files only), version_number.txt (shown on the title screen) and OpenMNOG.ini (where
#   the save goes).
#
# usage: bash release.sh [--allow-dirty]
#   --allow-dirty: build even with uncommitted changes in game/, src/, version_number.txt or OpenMNOG.ini (they're
#   left out of game/ but would be in the engine, so the release wouldn't match any commit).
cd "$(dirname "$0")"
fail() { echo "NOT BUILT: $*"; exit 1; }

VERSION=$(tr -d ' \r\n' < version_number.txt)
[[ "$VERSION" =~ ^[A-Za-z0-9._-]+$ ]] || fail "version_number.txt doesn't hold a version: '$VERSION'"
NAME="OpenMNOG-v$VERSION-win64"
OUT=build/release
STAGE="$OUT/$NAME"
ZIP="$OUT/$NAME.zip"

# What's released is what's committed.
if [ "$1" != "--allow-dirty" ]; then
  dirty=$(git status --porcelain -- game src version_number.txt OpenMNOG.ini | head -5)
  [ -z "$dirty" ] || fail "uncommitted changes (commit them, or pass --allow-dirty):
$dirty"
fi
[ -f game/game.json ] || fail "game/ has no game.json"

echo "Building $NAME ($(git rev-parse --short HEAD))"

# The engine: Windows x64, self-contained, one exe (its native libraries beside it).
rm -rf "$OUT/engine-win64" "$STAGE" "$ZIP"
dotnet publish src/Mnog.Engine -c Release -r win-x64 --self-contained true -o "$OUT/engine-win64" \
  -p:MnogRelease=true -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false -v q -nologo -clp:ErrorsOnly \
  || fail "engine publish failed"
[ -f "$OUT/engine-win64/Mnog.Engine.exe" ] || fail "no Mnog.Engine.exe in the publish output"

mkdir -p "$STAGE"
cp "$OUT/engine-win64/Mnog.Engine.exe" "$STAGE/OpenMNOG.exe"
for f in "$OUT/engine-win64"/*; do
  case "$(basename "$f")" in Mnog.Engine.exe|*.pdb) ;; *) cp -r "$f" "$STAGE/" ;; esac
done
cp version_number.txt OpenMNOG.ini "$STAGE/"

# The game files: those committed (git ls-files), not whatever else is lying in game/.
echo "Copying game files..."
git ls-files -z game | tar --null -T - -cf - | tar -xf - -C "$STAGE" \
  || fail "copying game/ failed"

# Zipped (deflate) with the folder at its root.
echo "Zipping..."
python - "$STAGE" "$ZIP" <<'EOF' || fail "zipping failed"
import os, sys, zipfile
stage, out = sys.argv[1], sys.argv[2]
root = os.path.dirname(stage)
with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
    for d, _, files in os.walk(stage):
        for f in sorted(files):
            p = os.path.join(d, f)
            z.write(p, os.path.relpath(p, root))
EOF

size=$(du -h "$ZIP" | cut -f1)
sum=$(sha256sum "$ZIP" | cut -d' ' -f1)
echo
echo "BUILT $ZIP ($size)"
echo "  files: $(find "$STAGE" -type f | wc -l), sha256 $sum"
echo "  Attach it to a GitHub release tagged v$VERSION (marked as the latest release), e.g.:"
echo "  gh release create v$VERSION \"$ZIP\" --title \"OpenMNOG v$VERSION\" --notes \"...\""
