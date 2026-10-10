#version 4
tellTarget ("../") {
    gotoAndPlay("Idle");
}
set("../../:EnemyActiveNum", eval("../../:EnemyActiveNum") - "1");
