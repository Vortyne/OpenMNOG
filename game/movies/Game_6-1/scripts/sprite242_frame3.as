#version 4
if (eval("../:EnemySpawnAllowed") eq "0") {
    RandNum = "2";
} else if (eval("../:BGAction") eq "Hive") {
    RandNum = random("25") + "1";
} else {
    RandNum = "2";
}
if (RandNum eq "1") {
    set("../:EnemyActiveNum", eval("../:EnemyActiveNum") + "1");
    Hit = "0";
    DiskScale = "10";
    gotoAndPlay(4);
} else {
    gotoAndPlay(1);
}
