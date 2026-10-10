#version 4
set("/:Button7", "0");
setTarget("/Holder/Button8");
if (eval("/:Button8") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button8", "1");
} else {
    set("/:Button8", "0");
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
gotoAndPlay(8);
