#version 4
Rollover = eval("../:Rollover");
if (Rollover eq "1") {
    tellTarget ("../") {
        Rollover = "0";
    }
    gotoAndPlay(3);
} else {
    gotoAndPlay(1);
}
