#version 4
if (eval("../:EnemySpawnAllowed") eq "0") {
    RandNum = "2";
} else if (eval("../:BGAction") eq "Canopy") {
    RandNum = random("35") + "1";
} else if (eval("../:BGAction") eq "Sky") {
    RandNum = random("45") + "1";
} else if (eval("../:BGAction") eq "Jungle") {
    RandNum = random("30") + "1";
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
