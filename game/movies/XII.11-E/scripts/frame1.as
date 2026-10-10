#version 4
if (eval("/:Quest") >= "5") {
    tellTarget ("Mask") {
        gotoAndStop(2);
    }
    tellTarget ("Nub") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
