#version 4
tellTarget ("/") {
    MovieName = "XII.7A.swf";
    set("/:LiftLoc", "Down");
    ShiftOff = "1";
    EndElevator = "1";
    BeginLoadMovie = "1";
}
stop();
