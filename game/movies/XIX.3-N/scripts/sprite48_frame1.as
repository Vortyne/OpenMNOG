#version 4
KoKoro = eval("/:KoKoro");
if (KoKoro eq "1") {
    tellTarget ("Kopeke") {
        gotoAndStop(2);
    }
} else if ("1" < KoKoro) {
    tellTarget ("Kopeke") {
        gotoAndStop(3);
    }
}
stop();
