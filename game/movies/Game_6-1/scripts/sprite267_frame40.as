#version 4
if (Hit eq "0") {
    tellTarget ("Enemy") {
        gotoAndStop("Down");
    }
}
if (Hit eq "1") {
    tellTarget ("Enemy") {
        gotoAndStop("Hit");
    }
    stop();
}
