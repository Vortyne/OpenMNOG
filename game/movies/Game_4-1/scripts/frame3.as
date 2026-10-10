#version 4
if (RemoveCollObj eq "1") {
    RemoveCollObj = "0";
    setProperty(eval("CollObj" add RemoveCollNum), _y, getProperty(eval("CollObj" add RemoveCollNum), _y) - "1000");
    set("CollObjHeight" add RemoveCollNum, "-10");
}
if (AllowFire eq "0") {
    setTarget("TimeMovie");
    TempTime = getTimer();
    if ("500" < TempTime - tCurrentTime) {
        set("../:AllowShoot", "1");
    }
    setTarget("");
}
if ("500" < getProperty("Scorpion", _x)) {
    if ("0" < xScorpionShift) {
        xScorpionShift = "0" - xScorpionShift;
    }
} else if (getProperty("Scorpion", _x) < "300") {
    if (xScorpionShift < "0") {
        xScorpionShift = "0" - xScorpionShift;
    }
}
if ("0" < LiveBalls) {
    GoalKeeping = "1";
} else {
    GoalKeeping = "0";
}
if (GoalKeeping eq "1") {
    if (getProperty("Boulder" add (BoulderCount - "1") add "/Boulder", _x) + getProperty("Boulder" add (BoulderCount - "1"), _x) < getProperty("Scorpion", _x) - "20") {
        GoalKeepingShift = "-10";
    } else if (getProperty("Scorpion", _x) + "20" < getProperty("Boulder" add (BoulderCount - "1") add "/Boulder", _x) + getProperty("Boulder" add (BoulderCount - "1"), _x)) {
        GoalKeepingShift = "10";
    } else {
        GoalKeepingShift = ("0" - getProperty("Scorpion", _x) + (getProperty("Boulder" add (BoulderCount - "1") add "/Boulder", _x) + getProperty("Boulder" add (BoulderCount - "1"), _x))) / "10";
    }
    setProperty("Scorpion", _x, getProperty("Scorpion", _x) + GoalKeepingShift);
} else {
    setProperty("Scorpion", _x, getProperty("Scorpion", _x) + xScorpionShift);
}
if (getProperty("Scorpion", _y) < yScorpion) {
    setProperty("Scorpion", _y, getProperty("Scorpion", _y) + "1");
} else if (yScorpion < getProperty("Scorpion", _y)) {
    setProperty("Scorpion", _y, getProperty("Scorpion", _y) - "1");
}
if (NumBricks - "1" < NumKnockedOver) {
    tellTarget ("/") {
        MovieName = "Episode_4-3.swf";
        BeginLoadMovie = "1";
    }
    stop();
} else {
    gotoAndPlay(2);
}
