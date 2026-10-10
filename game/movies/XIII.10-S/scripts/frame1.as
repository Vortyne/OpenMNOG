#version 4
if (eval("/:LeKoro") >= "1") {
    gotoAndPlay(7);
} else if (eval("/:OnuKoroLava") eq "1") {
    gotoAndPlay(4);
}
tellTarget ("/") {
    Startedloading = "1";
}
