#!/bin/bash
# release.sh: builds the release downloads into build/release/, to attach to the GitHub release tagged v{version} (the
# version from version_number.txt; the project page's download button links there):
#   OpenMNOG-v{version}-win64.zip          Windows (x64)
#   OpenMNOG-v{version}-linux64.tar.gz     Linux (x64)
#   OpenMNOG-v{version}-macos-arm64.zip    macOS on Apple silicon (M1 and later)
#   OpenMNOG-v{version}-macos-x64.zip      macOS on Intel
#
# Each holds one folder, OpenMNOG-v{version}-{target}/, with: the engine (OpenMNOG.exe, or OpenMNOG elsewhere;
# self-contained: players need no .NET) and its native libraries (Skia, SDL), game/ (the committed game files only),
# version_number.txt (shown on the title screen) and OpenMNOG.ini (where the save goes). Linux's and macOS's keep
# OpenMNOG executable.
#
# usage: bash release.sh [--allow-dirty] [TARGET...]   (targets: win64 linux64 macos-arm64 macos-x64, or all;
#                                                       win64 when none is given)
#   --allow-dirty: build from the working copy as it is, uncommitted changes and new files included (a test build:
#   it won't match any commit).
cd "$(dirname "$0")"
fail() { echo "NOT BUILT: $*"; exit 1; }

DIRTY=0
TARGETS=()
for a in "$@"; do
  case "$a" in
    --allow-dirty) DIRTY=1 ;;
    all) TARGETS+=(win64 linux64 macos-arm64 macos-x64) ;;
    win64|linux64|macos-arm64|macos-x64) TARGETS+=("$a") ;;
    *) fail "unknown argument: $a" ;;
  esac
done
[ ${#TARGETS[@]} -gt 0 ] || TARGETS=(win64)

VERSION=$(tr -d ' \r\n' < version_number.txt)
[[ "$VERSION" =~ ^[A-Za-z0-9._-]+$ ]] || fail "version_number.txt doesn't hold a version: '$VERSION'"
OUT=build/release

# What's released is what's committed.
if [ $DIRTY = 0 ]; then
  dirty=$(git status --porcelain -- game src version_number.txt OpenMNOG.ini | head -5)
  [ -z "$dirty" ] || fail "uncommitted changes (commit them, or pass --allow-dirty):
$dirty"
fi
[ -f game/game.json ] || fail "game/ has no game.json"
mkdir -p "$OUT"

# The game files: those committed (git ls-files), not whatever else is lying in game/. (--allow-dirty: also the
# new ones not yet committed, as they are now; ignored files still left out.)
if [ $DIRTY = 1 ]; then git ls-files -z --cached --others --exclude-standard game > "$OUT/game-files"
else git ls-files -z game > "$OUT/game-files"; fi

built=()
for target in "${TARGETS[@]}"; do
  case "$target" in
    win64) rid=win-x64; exe=Mnog.Engine.exe; as=OpenMNOG.exe; ext=zip ;;
    linux64) rid=linux-x64; exe=Mnog.Engine; as=OpenMNOG; ext=tar.gz ;;
    macos-arm64) rid=osx-arm64; exe=Mnog.Engine; as=OpenMNOG; ext=zip ;;
    macos-x64) rid=osx-x64; exe=Mnog.Engine; as=OpenMNOG; ext=zip ;;
  esac
  NAME="OpenMNOG-v$VERSION-$target"
  ARCHIVE="$OUT/$NAME.$ext"
  echo "Building $NAME ($(git rev-parse --short HEAD))"

  # The engine: self-contained, one executable (its native libraries beside it).
  rm -rf "$OUT/engine-$target" "$ARCHIVE"
  dotnet publish src/Mnog.Engine -c Release -r $rid --self-contained true -o "$OUT/engine-$target" \
    -p:MnogRelease=true -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false -v q -nologo -clp:ErrorsOnly \
    || fail "engine publish for $target failed"
  [ -f "$OUT/engine-$target/$exe" ] || fail "no $exe in the $target publish output"

  # Packed straight from the publish output and the repository (no copy of game/ first), the folder at its root.
  echo "Packing..."
  python - "$NAME" "$ARCHIVE" "$OUT/engine-$target" "$exe" "$as" "$OUT/game-files" <<'EOF' || fail "packing $target failed"
import os, sys, tarfile, zipfile
name, out, engine, exe, as_name, listing = sys.argv[1:]
# (source, path in the archive, executable)
items = [(os.path.join(engine, exe), as_name, True)]
items += [(os.path.join(engine, f), f, False) for f in sorted(os.listdir(engine))
          if f != exe and not f.endswith('.pdb') and os.path.isfile(os.path.join(engine, f))]
items += [(f, f, False) for f in ('version_number.txt', 'OpenMNOG.ini')]
items += [(f, f, False) for f in open(listing, 'rb').read().decode('utf-8').split('\0') if f and os.path.isfile(f)]
if out.endswith('.tar.gz'):
    with tarfile.open(out, 'w:gz', compresslevel=6) as t:
        for src, arc, x in items:
            info = t.gettarinfo(src, f'{name}/{arc}')
            info.mode, info.uid, info.gid, info.uname, info.gname = 0o755 if x else 0o644, 0, 0, '', ''
            with open(src, 'rb') as f:
                t.addfile(info, f)
else:
    with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
        for src, arc, x in items:
            info = zipfile.ZipInfo.from_file(src, f'{name}/{arc}')
            # (Unix permissions, so macOS's unzip keeps OpenMNOG runnable.)
            info.create_system = 3
            info.external_attr = (0o100755 if x else 0o100644) << 16
            info.compress_type = zipfile.ZIP_DEFLATED
            with open(src, 'rb') as f:
                z.writestr(info, f.read(), compresslevel=6)
print(f'{len(items)} files')
EOF
  built+=("$ARCHIVE")
done

echo
for a in "${built[@]}"; do echo "BUILT $a ($(du -h "$a" | cut -f1)), sha256 $(sha256sum "$a" | cut -d' ' -f1)"; done
echo "  Attach them to a GitHub release tagged v$VERSION (marked as the latest release), e.g.:"
echo "  gh release create v$VERSION ${built[*]} --title \"OpenMNOG v$VERSION\" --notes \"...\""
