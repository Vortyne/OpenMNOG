#version 4
if (eval("/:Quest") < "6") {
    if (eval("/:OnuKoroLava") eq "1") {
        gotoAndPlay(4);
    }
}
if (eval("/:Quest") >= "6") {
    tellTarget ("Middle-Ground/Tohunga") {
        gotoAndStop(2);
    }
}
if (eval("/:OnuKoroDoor") eq "1") {
    tellTarget ("/holder/Middle-Ground/Door") {
        gotoAndStop(3);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
