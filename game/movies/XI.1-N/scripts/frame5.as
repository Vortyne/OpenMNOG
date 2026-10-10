#version 4
if (eval("/:Quest") >= "6") {
    tellTarget ("Middle-Ground/Forward") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Crabs") {
        gotoAndStop(2);
    }
}
