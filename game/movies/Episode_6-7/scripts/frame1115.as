#version 4
if (eval("/:ExitLoop") eq "Done") {
    gotoAndPlay(1117);
} else if (frameLoaded(1544)) {
    set("/:ExitLoop", "1");
}
