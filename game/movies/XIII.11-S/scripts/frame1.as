#version 4
if (eval("/:OnuKoroLava") eq "1") {
    gotoAndPlay(4);
}
TurnBack = eval("/:TurnBack");
if (TurnBack eq "1") {
    tellTarget ("/holder/Fore-Ground/Taipu") {
        gotoAndPlay("TurnBack");
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
