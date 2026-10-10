#version 4
TurnBack = eval("/:TurnBack");
if (TurnBack eq "1") {
    tellTarget ("/holder/Middle-Ground/Whenua") {
        gotoAndPlay("TurnBack");
    }
}
setProperty("Middle-Ground/Door", _y, "-3000");
setProperty("Middle-Ground/GuildBubble", _y, "-3000");
if (eval("/:OnuKoroDoor") eq "1") {
    setProperty("Middle-Ground/Rats", _y, "-3000");
    setProperty("Middle-Ground/GuardBubble", _y, "-3000");
}
