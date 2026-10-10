#version 4
if (eval("../:Hit") eq "0" and eval("../../:AllowFire") eq "1") {
    set("../../:EnemyActiveNum", eval("../../:EnemyActiveNum") - "1");
    set("../:Hit", "1");
    set("../../:Shooting", "1");
    set("../../FiringTween/:DiskScale", eval("../:DiskScale"));
    set("../../:xFireTween", eval("../:Factor") * getProperty("../AimTarget", _x) + eval("../:xOffset"));
    set("../../:yFireTween", getProperty("../AimTarget", _y) + eval("../:yOffset"));
    tellTarget ("../../FiringTween") {
        gotoAndPlay("shooting");
    }
    tellTarget ("../TellHit") {
        gotoAndPlay("play");
    }
}
