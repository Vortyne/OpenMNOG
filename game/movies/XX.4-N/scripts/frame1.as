#version 4
if (eval("/:KoKoro") eq "7") {
    tellTarget ("Back-Ground/Tohunga") {
        gotoAndStop(2);
    }
}
if (eval("/:KoKoro") >= "6") {
    tellTarget ("Middle-Ground/Door") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
