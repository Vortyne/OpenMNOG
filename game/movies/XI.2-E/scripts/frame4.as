#version 4
TurnBack = eval("/:TurnBack");
if (TurnBack eq "1") {
    tellTarget ("/holder/Middle-Ground/Onepu") {
        gotoAndPlay("TurnBack");
    }
}
if (eval("/:Quest") >= "6") {
    tellTarget ("Middle-Ground/Onepu") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
