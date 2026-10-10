#version 4
set("/:Button6", "0");
setTarget("/Holder/Button3");
if (eval("/:Button3") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button3", "1");
} else {
    set("/:Button3", "0");
    gotoAndPlay("PlayOff");
}
setTarget("");
setTarget("/Holder/Button5");
if (eval("/:Button5") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button5", "1");
} else {
    set("/:Button5", "0");
    gotoAndPlay("PlayOff");
}
setTarget("");
setTarget("/Holder/Button9");
if (eval("/:Button9") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button9", "1");
} else {
    set("/:Button9", "0");
    gotoAndPlay("PlayOff");
}
setTarget("");
gotoAndPlay(8);
