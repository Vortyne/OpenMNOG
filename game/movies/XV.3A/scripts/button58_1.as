#version 4
set("/:Button5", "1");
setTarget("/Holder/Button2");
if (eval("/:Button2") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button2", "1");
} else {
    set("/:Button2", "0");
    gotoAndPlay("PlayOff");
}
setTarget("");
setTarget("/Holder/Button4");
if (eval("/:Button4") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button4", "1");
} else {
    set("/:Button4", "0");
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
setTarget("/Holder/Button8");
if (eval("/:Button8") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button8", "1");
} else {
    set("/:Button8", "0");
    gotoAndPlay("PlayOff");
}
setTarget("");
gotoAndPlay(3);
