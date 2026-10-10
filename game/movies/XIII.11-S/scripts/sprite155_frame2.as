#version 4
Digging = eval("../:Digging");
if (Digging eq "1") {
    tellTarget ("../") {
        Digging = "0";
    }
    gotoAndPlay(3);
} else {
    gotoAndPlay(1);
}
