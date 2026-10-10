#version 4
if (eval("/:Quest") < "6") {
    if (eval("/:OnuKoroLava") eq "1") {
        gotoAndPlay(4);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
