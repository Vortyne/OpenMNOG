#version 4
tellTarget ("/") {
    MovieName = "XXI.8-N.swf";
    VerticalShift = "0";
    HorizontalShift = "1";
    BeginLoadMovie = "1";
}
set("/:FGFlag", "0");
set("/:FGShrine", "0");
set("/:Shrine", "0");
if (eval("/:BGShrine") eq "1") {
    set("/:Flag", "2");
    set("/:BGShrine", "0");
}
if (eval("/:Flag") eq "6") {
    set("/:Flag", "7");
}
