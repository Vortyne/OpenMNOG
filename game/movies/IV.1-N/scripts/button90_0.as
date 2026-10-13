#version 4
WalkClick = eval("../:WalkClick");
TurnClick = eval("../:TurnClick");
AllowClick = eval("../:AllowClick");
if (eval("../:WalkClick") eq "0") {
    tellTarget ("../") {
        WalkClick = "1";
    }
} else if (getProperty("../TurnStop", _currentframe) >= "11" and getProperty("../TurnStop", _currentframe) <= "12") {
    // (OpenMNOG: closing what he says only once he's said it (his bubble open, facing you): a second click while he
    // turned (a double click) closed the bubble as soon as it opened.)
    tellTarget ("../") {
        TurnClick = "1";
    }
}
