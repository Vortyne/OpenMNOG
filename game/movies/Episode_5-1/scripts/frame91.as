#version 4
Destination = eval("/:Destination");
tellTarget ("/") {
    MovieName = Destination;
    ShiftOff = "0";
    BeginLoadMovie = "1";
}
stop();
