#version 4
count = count + "0.5";
setProperty("Top", _y, yTopStart + count);
if ("170" < count) {
    gotoAndPlay(13);
} else if ("160" < count) {
    set("../../:EnemySpawnAllowed", "0");
} else {
    gotoAndPlay(11);
}
