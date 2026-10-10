#version 4
set("/:Button3", "0");
setTarget("/Holder/Button2");
if (eval("/:Button2") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button2", "1");
} else {
    set("/:Button2", "0");
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
