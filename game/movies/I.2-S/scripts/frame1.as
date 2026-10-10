#version 4
TaVisit = eval("/:TaVisit");
if (TaVisit eq "0") {
    tellTarget ("Middle-Ground/ Boat") {
        gotoAndStop(2);
    }
}
if (eval("/:Quest") >= "6") {
    tellTarget ("Back-Ground/Forward") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Forward") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Vakama") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/ Boat") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
Rollover1Playing = "0";
loadMovie("sky1.swf", "/holder/Back-Ground/Sky-holder", "GET");
GaKoro = eval("/:GaKoro");
GaKoroVisited = eval("/:GaKoroVisited");
if (TaVisit eq "0" or GaKoro eq "1" or GaKoroVisited eq "1") {
    setProperty("Middle-Ground/Maku", _x, "3000");
}
