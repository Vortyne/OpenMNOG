#version 4
if (eval("/:fLeKoro") eq "10") {
    tellTarget ("Fore-Ground/Pod") {
        gotoAndStop(2);
    }
}
if (eval("/:cKapura") eq "1") {
    tellTarget ("Middle-Ground/Tamaru") {
        gotoAndStop(2);
    }
}
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:cTamaru") eq "1") {
    tellTarget ("Middle-Ground/Tamaru") {
        gotoAndStop(1);
    }
}
