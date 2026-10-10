#version 4
set("/:Button8", "0");
setTarget("/Holder/Button9");
if (eval("/:Button9") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button9", "1");
} else {
    set("/:Button9", "0");
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
setTarget("/Holder/Button7");
if (eval("/:Button7") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button7", "1");
} else {
    set("/:Button7", "0");
    gotoAndPlay("PlayOff");
}
setTarget("");
gotoAndPlay(8);
