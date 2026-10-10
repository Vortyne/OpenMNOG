#version 4
set("/:Button1", "1");
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
gotoAndPlay(3);
