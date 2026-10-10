#version 4
if (eval("/:Quest") >= "6") {
    tellTarget ("Forward") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
