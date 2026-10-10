#version 4
if (eval("/:Quest") >= "6") {
    tellTarget ("Back-Ground/Tohunga") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
