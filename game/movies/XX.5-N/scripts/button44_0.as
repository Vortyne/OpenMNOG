#version 4
tellTarget ("/") {
    setProperty("/Holder/TurnButtons", _y, "-1000");
}
tellTarget ("/Holder/Middle-ground/Tohunga/Nuju") {
    gotoAndPlay("TurnTo");
}
gotoAndPlay(43);
