#version 4
if (eval("/:Quest") >= "5") {
    tellTarget ("Middle-Ground/Gate") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
