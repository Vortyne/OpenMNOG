#version 4
set("../../:EnemyActiveNum", eval("../../:EnemyActiveNum") - "1");
tellTarget ("../") {
    gotoAndPlay("Idle");
}
stop();
