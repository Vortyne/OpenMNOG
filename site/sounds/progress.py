"""
How much of the game's sound plays in high quality, for the sound list page: counted from the game itself (no workspace
needed), so the page's percentage follows game/audio/hq/hq.json as sounds are added. Each unique sound file in the game's
movies (the same file in several movies counted once) is in high quality when hq.json replaces any of its copies.
Writes {"total", "hq"} into the sounds.json given (the Pages workflow runs it on the published copy).

usage: python site/sounds/progress.py [SOUNDS_JSON]   (from the repository's root; default site/sounds/sounds.json)
"""
import glob, hashlib, json, os, sys

hq = json.load(open('game/audio/hq/hq.json', encoding='utf-8'))['sounds']
copies = {}
for f in glob.glob('game/movies/*/sounds/*.ogg'):
    parts = f.replace(os.sep, '/').split('/')
    key = f'{parts[2]}#{os.path.splitext(parts[-1])[0]}'
    copies.setdefault(hashlib.md5(open(f, 'rb').read()).hexdigest(), []).append(key)
progress = {'total': len(copies), 'hq': sum(any(k in hq for k in keys) for keys in copies.values())}

path = sys.argv[1] if len(sys.argv) > 1 else 'site/sounds/sounds.json'
doc = json.load(open(path, encoding='utf-8'))
doc['progress'] = progress
json.dump(doc, open(path, 'w', encoding='utf-8', newline='\n'), indent=1, ensure_ascii=False)
print(f"{progress['hq']} of {progress['total']} sounds in high quality ({progress['hq'] / progress['total']:.0%})")
