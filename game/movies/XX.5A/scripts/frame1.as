#version 4
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:KoKoro") >= "6") {
    tellTarget ("Door") {
        gotoAndStop("Open");
    }
}
