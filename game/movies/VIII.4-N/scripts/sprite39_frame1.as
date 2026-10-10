#version 4
PlayRollover1 = "0";
VIII4NVisited = eval("/:VIII4NVisited");
KohliBall = eval("/:KohliBall");
KohliBallUsed = eval("/:KohliBallUsed");
if (KohliBall eq "1" or KohliBallUsed eq "1") {
    setProperty("KohliBall", _y, "-2000");
    setProperty("BallPlayer", _y, "-2000");
    tellTarget ("BallPlayer") {
        gotoAndStop("Idle");
    }
}
if ("1" < VIII4NVisited) {
    setProperty("BallPlayer", _y, "-2000");
    tellTarget ("BallPlayer") {
        gotoAndStop("Idle");
    }
}
