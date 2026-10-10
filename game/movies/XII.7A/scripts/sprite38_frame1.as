#version 4
if (eval("/:EndElevator") eq "1") {
    tellTarget ("/") {
        EndElevator = "0";
    }
    gotoAndPlay(3);
}
