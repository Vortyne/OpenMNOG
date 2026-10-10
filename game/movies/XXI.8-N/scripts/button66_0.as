#version 4
if (eval("/:FGShrine") eq "1") {
    set("/:FGShrine", "2");
} else if (eval("/:FGShrine") eq "2") {
    set("/:FGShrine", "1");
}
if (eval("/:Flag") eq "3") {
    if (eval("/:BGShrine") eq "1") {
        set("/:Flag", "2");
        set("/:BGShrine", "0");
    }
}
tellTarget ("/") {
    MovieName = "XXI.8-N.swf";
    VerticalShift = "0";
    HorizontalShift = "1";
    BeginLoadMovie = "1";
}
