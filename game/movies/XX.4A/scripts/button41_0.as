#version 4
setTarget("Tohunga");
if (eval("/:KoKoro") < "6") {
    gotoAndPlay("Open");
} else if (eval("/:KoKoro") eq "6") {
    gotoAndPlay("Point");
} else if (eval("/:KoKoro") eq "7") {
    gotoAndPlay("Point2");
}
setTarget("");
