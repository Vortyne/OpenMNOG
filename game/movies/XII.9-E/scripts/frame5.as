#version 4
if (eval("/:OnuKoroDoor") eq "1") {
    tellTarget ("/holder/Middle-Ground/Door") {
        gotoAndStop(3);
    }
} else {
    tellTarget ("/holder/Middle-Ground/Door") {
        gotoAndStop(2);
    }
}
TurnBack = eval("/:TurnBack");
if (TurnBack eq "1") {
    tellTarget ("/holder/Middle-Ground/Prospector") {
        gotoAndPlay("TurnBack");
    }
}
if (eval("/:cTaipu") eq "1") {
    setProperty("Middle-Ground/Taipu", _x, "2000");
} else if (eval("/:cKapura") eq "1") {
    if (eval("/:TaipuCanGo") ne "1") {
        setTarget("/");
        if (Taipu ne "cBar") {
            TaipuWas = Taipu;
        }
        Taipu = "cBar";
        setTarget("");
    }
} else {
    setProperty("Middle-Ground/Taipu", _x, "2000");
}
