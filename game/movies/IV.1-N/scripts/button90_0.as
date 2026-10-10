#version 4
WalkClick = eval("../:WalkClick");
TurnClick = eval("../:TurnClick");
AllowClick = eval("../:AllowClick");
if (eval("../:WalkClick") eq "0") {
    tellTarget ("../") {
        WalkClick = "1";
    }
} else {
    tellTarget ("../") {
        TurnClick = "1";
    }
}
