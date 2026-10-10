#version 4
if (eval("/:OnuKoroLava") eq "1") {
    tellTarget ("Middle-Ground/BG_Art") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
