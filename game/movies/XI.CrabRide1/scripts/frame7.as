#version 4
set("/:Exit", "0");
set("/:CrabGo", "1");
set("/:Speed", "0");
set("/:Destination", "0");
if (eval("/:CrabFrom") eq "PoKoro") {
    gotoAndPlay(16);
} else if (eval("/:CrabFrom") eq "OnuKoro") {
    gotoAndPlay(30);
}
