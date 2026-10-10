#version 4
tellTarget ("TimeMovie") {
    tShootTime = getTimer();
}
if (AllowShoot eq "1") {
    tellTarget ("Pohatu/Pohatu") {
        gotoAndPlay("SwingBack");
    }
    AllowShoot = "0";
    AllowFire = "1";
}
