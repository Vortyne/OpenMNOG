#version 4
if (eval("/:TaVisit") eq "0") {
    tellTarget ("Middle-Ground/Boat") {
        gotoAndStop(2);
    }
}
if (eval("/:Quest") >= "6") {
    tellTarget ("Middle-Ground/Forward") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Vakama") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Boat") {
        gotoAndStop(2);
    }
}
GaKoro = eval("/:GaKoro");
GaKoroVisited = eval("/:GaKoroVisited");
if (eval("/:TaVisit") eq "0" or GaKoro eq "1" or GaKoroVisited eq "1") {
    setProperty("Middle-Ground/Maku", _x, "3000");
}
if (Skyloaded eq "0") {
    gotoAndPlay(2);
}
