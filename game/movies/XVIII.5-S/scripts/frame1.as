#version 4
if (eval("/:LeKoro") eq "1") {
} else if (eval("/:LeKoro") eq "3") {
    tellTarget ("Back-Ground/Nest") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Tohunga") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Tamaru") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Doors") {
        gotoAndStop(2);
    }
    tellTarget ("Back-Ground/Kewa") {
        gotoAndStop(2);
    }
} else if (eval("/:LeKoro") eq "4") {
    tellTarget ("Back-Ground/Nest") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Tohunga") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Doors") {
        gotoAndStop(2);
    }
    tellTarget ("Back-Ground/Kewa") {
        gotoAndStop(2);
    }
} else if (eval("/:LeKoro") eq "5") {
    tellTarget ("Back-Ground/Nest") {
        gotoAndStop(1);
    }
    tellTarget ("Middle-Ground/Doors") {
        gotoAndStop(2);
    }
    tellTarget ("Middle-Ground/Tamaru") {
        gotoAndStop(3);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
