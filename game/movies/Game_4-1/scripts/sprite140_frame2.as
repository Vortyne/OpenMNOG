#version 4
setProperty("Boulder", _x, getProperty("Boulder", _x) + xVelocity);
setProperty("Boulder", _y, getProperty("Boulder", _y) + yVelocity);
if (BoulderNum eq "1") {
    trace("yVel is " add yVelocity);
}
if ("700" - xPohatu < getProperty("Boulder", _x)) {
    xVelocity = ("0" - xVelocity) * "0.8";
    setProperty("Boulder", _x, getProperty("Boulder", _x) + "2" * xVelocity);
    tellTarget ("WallBounce") {
        gotoAndPlay("Play");
    }
} else if (getProperty("Boulder", _x) < "70" - xPohatu) {
    xVelocity = ("0" - xVelocity) * "0.8";
    setProperty("Boulder", _x, getProperty("Boulder", _x) + "2" * xVelocity);
    tellTarget ("WallBounce") {
        gotoAndPlay("Play");
    }
}
if (getProperty("Boulder", _y) < "100" - yPohatu) {
    yVelocity = ("0" - yVelocity) * "0.4";
    setProperty("Boulder", _y, getProperty("Boulder", _y) + "2" * yVelocity);
}
ScaleDistanceShift = getProperty("Boulder", _y) / "10";
ScaleMin = "100" + ScaleDistanceShift;
ScaleMax = "100" + yVelocity * "-2" + ScaleDistanceShift;
if (ScaleMax < ActualScale) {
    if ("3" < LoftCount) {
        if ("0" < ScaleCounter) {
            ScaleCounter = ScaleCounter * "-1";
        }
        LoftCount = "1";
        Lofting = "0";
    } else {
        Lofting = "1";
        LoftCount = LoftCount + "1";
        ActualScale = ScaleMax;
    }
} else if (ActualScale < ScaleMin) {
    if (ScaleCounter < "0") {
        ScaleCounter = ScaleCounter * "-1";
    }
    xVelocity = xVelocity * "0.5";
    yVelocity = yVelocity * "0.5";
}
if ("-1" < yVelocity) {
    if (Collisions eq "1") {
        if (Break eq "0") {
            Break = "1";
            Collisions = "0";
            if ("0" < BoulderNum) {
                tellTarget ("Boulder") {
                    gotoAndPlay("BreakApart");
                }
            }
        }
    }
    xVelocity = "0";
    yVelocity = "0";
    setProperty("Boulder", _xscale, ScaleMin);
    setProperty("Boulder", _yscale, ScaleMin);
} else {
    if (Lofting eq "0") {
        ActualScale = ActualScale + ScaleCounter;
    }
    setProperty("Boulder", _xscale, ActualScale);
    setProperty("Boulder", _yscale, ActualScale);
}
if (Collisions eq "1") {
    CollCount = "0";
    while (CollCount < CollObjs) {
        CollCount = CollCount + "1";
        if (ActualScale - ScaleMin < eval("../:CollObjHeight" add CollCount)) {
            if (eval("../:CollObjScorp" add CollCount) eq "1") {
                xObj = "0" - xPohatu + getProperty("../Scorpion", _x) + getProperty("../" add eval("../:CollObj" add CollCount), _x);
                yObj = "0" - yPohatu + getProperty("../Scorpion", _y) + getProperty("../" add eval("../:CollObj" add CollCount), _y);
                Scorpion = "1";
            } else {
                xObj = "0" - xPohatu + getProperty("../" add eval("../:CollObj" add CollCount), _x);
                yObj = "0" - yPohatu + getProperty("../" add eval("../:CollObj" add CollCount), _y);
                Scorpion = "0";
            }
            xObjRadius = getProperty("../" add eval("../:CollObj" add CollCount), _width) / "2";
            yObjRadius = getProperty("../" add eval("../:CollObj" add CollCount), _height) / "2";
            xBoulder = getProperty("Boulder", _x);
            yBoulder = getProperty("Boulder", _y);
            if (xBoulder < xObj + xObjRadius and xObj - xObjRadius < xBoulder) {
                if (yBoulder < yObj + yObjRadius and yObj - yObjRadius < yBoulder) {
                    if (yVelocity < "0") {
                        yVelocity = ("0" - yVelocity) * "0.4";
                        Collisions = "0";
                        tellTarget ("Boulder") {
                            gotoAndPlay("BreakApart");
                        }
                        Break = "1";
                        trace("Collision with " add eval("../:CollObj" add CollCount));
                        tellTarget ("../" add eval("../:CollObj" add CollCount)) {
                            play = play + "1";
                        }
                        if (Scorpion eq "1") {
                            setProperty("../Scorpion", _y, getProperty("../Scorpion", _y) - "10");
                            tellTarget ("../Scorpion/Tail") {
                                gotoAndPlay("TailSwing");
                            }
                        }
                    }
                }
            }
        }
    }
}
if (Break eq "1" and "0" < BoulderNum) {
    if (Goaltrack < "1") {
        Goaltrack = "1";
        set("../:LiveBalls", eval("../:LiveBalls") - "1");
    }
}
if (DeleteBall eq "1") {
    set("../:BoulderDelete", BoulderNum);
    tellTarget ("../") {
        removeMovieClip("Boulder" add BoulderDelete);
    }
    stop();
}
