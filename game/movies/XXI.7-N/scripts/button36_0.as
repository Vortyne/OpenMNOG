#version 4
setTarget("/");
if (eval("/:KoKoro") eq "7") {
    MovieName = "XXI.6-S.swf";
    VerticalShift = "0";
    HorizontalShift = "1";
} else {
    MovieName = "XXI.7-S.swf";
    VerticalShift = "1";
    HorizontalShift = "0";
}
BeginLoadMovie = "1";
setTarget("");
