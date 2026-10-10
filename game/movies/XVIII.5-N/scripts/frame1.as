#version 4
if (eval("/:Lekoro") eq "2") {
    tellTarget ("Middle-Ground/Tohunga") {
        gotoAndStop(2);
    }
} else if (eval("/:Lekoro") eq "5") {
    tellTarget ("Middle-Ground/Tohunga") {
        gotoAndStop(3);
    }
}
if (eval("/:Taipu") eq "Free") {
    tellTarget ("Middle-Ground/Taipu") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
