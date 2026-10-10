#version 4
set("/:SlideX", "3");
set("/:SlideY", "2");
if (eval("/:Taipu") eq "Caught") {
    tellTarget ("Hawk/Target/Taipu") {
        gotoAndStop(2);
    }
}
if (eval("/:Taipu") eq "Free") {
    tellTarget ("Hawk/Target/Taipu") {
        gotoAndStop(2);
    }
}
