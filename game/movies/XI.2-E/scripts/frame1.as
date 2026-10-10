#version 4
if (eval("/:OnuKoroLava") eq "1") {
    gotoAndPlay(4);
}
TurnBack = eval("/:TurnBack");
if (TurnBack eq "1") {
    tellTarget ("/holder/Middle-Ground/Onepu") {
        gotoAndPlay("TurnBack");
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
