#version 4
if (Hit eq "1") {
    tellTarget ("Enemy") {
        gotoAndPlay("Hit");
    }
    stop();
}
