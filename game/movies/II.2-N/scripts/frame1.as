#version 4
II2NVisited = eval("/:II2NVisited");
if (II2NVisited eq "1") {
    gotoAndPlay(110);
    tellTarget ("/") {
        VerticalShift = "1";
        HorizontalShift = "0";
    }
} else {
    tellTarget ("/") {
        VerticalShift = "0";
        HorizontalShift = "0";
        ShiftOff = "1";
    }
}
