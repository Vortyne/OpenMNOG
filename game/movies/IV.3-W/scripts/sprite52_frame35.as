#version 4
Rolloff = eval("../:Rolloff");
if (Rolloff eq "1") {
    tellTarget ("../") {
        Rolloff = "0";
    }
    gotoAndPlay(1);
} else {
    gotoAndPlay(3);
}
