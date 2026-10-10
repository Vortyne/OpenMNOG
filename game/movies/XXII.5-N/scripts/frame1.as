#version 4
if (eval("/:Quest") >= "2") {
    tellTarget ("Back-Ground/Forward") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
