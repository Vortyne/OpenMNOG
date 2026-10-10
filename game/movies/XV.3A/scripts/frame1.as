#version 4
set("/:Button1", random("2"));
setTarget("/Holder/Button1");
if (eval("/:Button1") eq "0") {
    gotoAndPlay("Off");
} else {
    gotoAndPlay("On");
}
setTarget("");
set("/:Button2", random("2"));
setTarget("/Holder/Button2");
if (eval("/:Button2") eq "0") {
    gotoAndPlay("Off");
} else {
    gotoAndPlay("On");
}
setTarget("");
set("/:Button3", random("2"));
setTarget("/Holder/Button3");
if (eval("/:Button3") eq "0") {
    gotoAndPlay("Off");
} else {
    gotoAndPlay("On");
}
setTarget("");
set("/:Button4", random("2"));
setTarget("/Holder/Button4");
if (eval("/:Button4") eq "0") {
    gotoAndPlay("Off");
} else {
    gotoAndPlay("On");
}
setTarget("");
set("/:Button5", random("2"));
setTarget("/Holder/Button5");
if (eval("/:Button5") eq "0") {
    gotoAndPlay("Off");
} else {
    gotoAndPlay("On");
}
setTarget("");
set("/:Button6", random("2"));
setTarget("/Holder/Button6");
if (eval("/:Button6") eq "0") {
    gotoAndPlay("Off");
} else {
    gotoAndPlay("On");
}
setTarget("");
set("/:Button7", random("2"));
setTarget("/Holder/Button7");
if (eval("/:Button7") eq "0") {
    gotoAndPlay("Off");
} else {
    gotoAndPlay("On");
}
setTarget("");
set("/:Button8", random("2"));
setTarget("/Holder/Button8");
if (eval("/:Button8") eq "0") {
    gotoAndPlay("Off");
} else {
    gotoAndPlay("On");
}
setTarget("");
set("/:Button9", random("2"));
setTarget("/Holder/Button9");
if (eval("/:Button9") eq "0") {
    gotoAndPlay("Off");
} else {
    gotoAndPlay("On");
}
setTarget("");
tellTarget ("/") {
    Startedloading = "1";
}
