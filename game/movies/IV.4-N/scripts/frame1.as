#version 4
loadMovie("sky2.swf", "/holder/Back-Ground/Sky-holder");
Winch = eval("/:Winch");
TaBridge = eval("/:TaBridge");
IV4NRaiseWinch = eval("/:IV4NRaiseWinch");
IV7ERaiseWinch = eval("/:IV7ERaiseWinch");
if (IV4NRaiseWinch eq "1") {
    tellTarget ("/") {
        IV4NRaiseWinch = "0";
    }
    if (TaBridge eq "1") {
        gotoAndPlay(116);
    } else {
        gotoAndPlay(3);
    }
} else if (IV7ERaiseWinch eq "1") {
    tellTarget ("/") {
        IV7ERaiseWinch = "0";
    }
    if (TaBridge eq "1") {
        gotoAndPlay(116);
    } else {
        gotoAndPlay(3);
    }
}
if (TaBridge eq "1") {
    setProperty("/holder/Back-Ground/Button-Castle", _x, "-69.1");
    setProperty("/holder/Back-Ground/Button-Castle", _y, "161.6");
    setProperty("/holder/Back-Ground/Gates-Shut", _x, "2000");
    setProperty("/holder/Back-Ground/Steps1", _x, "-51.5");
    setProperty("/holder/Back-Ground/Steps1", _y, "404.2");
    setProperty("/holder/Middle-Ground/Steps2", _x, "38.3");
    setProperty("/holder/Middle-Ground/Steps2", _y, "298.4");
    setProperty("/holder/Middle-Ground/Steps3", _x, "32.4");
    setProperty("/holder/Middle-Ground/Steps3", _y, "398.8");
}
tellTarget ("/") {
    Startedloading = "1";
}
