#version 4
TurnBack = eval("/:TurnBack");
if (TurnBack eq "1") {
    tellTarget ("/holder/Middle-Ground/UssalKeeper") {
        gotoAndPlay("TurnBack");
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
