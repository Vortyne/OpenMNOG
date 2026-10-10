#version 4
tellTarget ("/") {
    Movieisloaded = "1";
}
if (eval("/:CrabFrom") eq "PoKoro") {
    gotoAndPlay(16);
} else if (eval("/:CrabFrom") eq "OnuKoro") {
    gotoAndPlay(4);
}
gotoAndPlay(16);
