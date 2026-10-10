#version 4
if (eval("/:KoKoro") eq "7") {
    tellTarget ("/") {
        setProperty("/Holder/TurnButtons", _y, "-1000");
    }
    tellTarget ("/Holder/Middle-ground/Tohunga/Matoro") {
        gotoAndPlay("TurnTo");
    }
    gotoAndPlay(32);
}
