#version 4
set("/:Button9", "0");
setTarget("/Holder/Button8");
if (eval("/:Button8") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button8", "1");
} else {
    set("/:Button8", "0");
    gotoAndPlay("PlayOff");
}
setTarget("");
setTarget("/Holder/Button6");
if (eval("/:Button6") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button6", "1");
} else {
    set("/:Button6", "0");
    gotoAndPlay("PlayOff");
}
setTarget("");
gotoAndPlay(8);
