#version 4
if (eval("/:Quest") < "6") {
    if (eval("/:OnuKoroLava") eq "1") {
        gotoAndPlay(4);
    }
}
if (eval("/:Quest") >= "6") {
    tellTarget ("Fore-Ground/Tohunga") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
