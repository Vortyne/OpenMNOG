#version 4
if (eval("/:LeKoro") eq "3") {
    tellTarget ("Kite") {
        gotoAndStop(2);
    }
} else if (eval("/:LeKoro") eq "5") {
    tellTarget ("Kite") {
        gotoAndStop(3);
    }
    tellTarget ("Rope") {
        gotoAndStop(2);
    }
}
