#version 4
Hit = eval("../:Hit");
Miss = eval("../:Miss");
if (Hit eq "1") {
    tellTarget ("../") {
        gotoAndPlay("Hit");
    }
} else if (Miss eq "1") {
    tellTarget ("../") {
        gotoAndPlay("Miss");
    }
}
