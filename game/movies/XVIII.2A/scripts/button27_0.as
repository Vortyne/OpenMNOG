#version 4
setTarget("/");
if (eval("/:LiftLoc") eq "Up") {
    MovieName = "XVIII.3-S.swf";
} else {
    MovieName = "XVIII.1-N.swf";
}
VerticalShift = "0";
HorizontalShift = "1";
ShiftOff = "0";
BeginLoadMovie = "1";
setTarget("");
