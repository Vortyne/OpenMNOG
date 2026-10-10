#version 4
if (AllowFire eq "1") {
    Shooting = "1";
    set("FiringTween/:DiskScale", "5");
    xFireTween = getProperty("/MouseDrag", _x);
    yFireTween = getProperty("/MouseDrag", _y);
    tellTarget ("FiringTween") {
        gotoAndPlay("missing");
    }
}
