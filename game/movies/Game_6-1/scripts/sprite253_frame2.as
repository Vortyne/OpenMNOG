#version 4
if (eval("../:EnemySpawnAllowed") eq "0") {
    RandNum = "2";
} else if (eval("../:BGAction") eq "Jungle") {
    RandNum = random("50") + "1";
} else {
    RandNum = "2";
}
if (RandNum eq "1") {
    set("../:EnemyActiveNum", eval("../:EnemyActiveNum") + "1");
    Hit = "0";
    DiskScale = "30";
    gotoAndPlay(3);
} else {
    gotoAndPlay(1);
}
