#version 4
set("/:Button4", "0");
setTarget("/Holder/Button1");
if (eval("/:Button1") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button1", "1");
} else {
    set("/:Button1", "0");
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
