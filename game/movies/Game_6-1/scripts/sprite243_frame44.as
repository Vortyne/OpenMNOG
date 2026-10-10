#version 4
if (Hit eq "0") {
    tellTarget ("Enemy") {
        gotoAndStop("Strafe_Left");
    }
}
set("../:EnemyActiveNum", eval("../:EnemyActiveNum") - "1");
