#version 4
set("/:Button2", "0");
setTarget("/Holder/Button1");
if (eval("/:Button1") eq "0") {
    gotoAndPlay("PlayOn");
    set("/:Button1", "1");
} else {
    set("/:Button1", "0");
    gotoAndPlay("PlayOff");
}
setTarget("");
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
gotoAndPlay(8);
