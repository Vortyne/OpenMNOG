#version 4
tellTarget ("/") {
    MovieName = "XXI.8-N.swf";
    VerticalShift = "0";
    HorizontalShift = "1";
    BeginLoadMovie = "1";
}
set("/:Flag", eval("/:Flag") + "1");
set("/:FGFlag", "1");
