#version 4
setProperty("../Fish" add WhichFish, _xscale, "100");
setProperty("../Fish" add WhichFish, _yscale, "100");
xAvgPos = eval("../:xAvgPos");
yAvgPos = eval("../:yAvgPos");
xPosition = getProperty("../Fish" add WhichFish, _x);
yPosition = getProperty("../Fish" add WhichFish, _y);
if (ScreenXmax - PushBar < xPosition) {
    if (ScreenXmax - PushThreshold < xPosition) {
        xVelocity = "0" - AccelConst;
    } else {
        xAcceleration = xVelocity * (PushBar / "1000" / (xPosition - ScreenXmax)) - AccelConst;
        xVelocity = xVelocity + xAcceleration;
    }
} else if (xPosition < ScreenXmin + PushBar) {
    if (xPosition < ScreenXmin + PushThreshold) {
        xVelocity = AccelConst;
    } else {
        xAcceleration = xVelocity * (PushBar / "1000" / (xPosition - ScreenXmin)) + AccelConst;
        xVelocity = xVelocity + xAcceleration;
    }
} else if (xPosition + "50" < xAvgPos) {
    xVelocity = xVelocity + (xAvgPos - xPosition) / "2500" * MaxVel;
} else if (xAvgPos < xPosition - "50") {
    xVelocity = xVelocity + (xAvgPos - xPosition) / "2500" * MaxVel;
} else {
    xAvgVel = eval("../:xAvgVel");
    if (xAvgVel * "1.1" < xVelocity) {
        xVelocity = xVelocity + "0.1" * xAvgVel;
    } else if (xVelocity < xAvgVel * "0.909090909090909") {
        xVelocity = xVelocity + "0.1" * xAvgVel;
    } else {
        xVelocity = xVelocity + "0.2" * (random("35") - "17");
    }
}
if (ScreenYmax - PushBar < yPosition) {
    if (ScreenYmax - PushThreshold < yPosition) {
        yVelocity = "0" - AccelConst;
    } else {
        yAcceleration = yVelocity * (PushBar / "1000" / (yPosition - ScreenYmax)) - AccelConst;
        yVelocity = yVelocity + yAcceleration;
    }
} else if (yPosition < ScreenYmin + PushBar) {
    if (yPosition < ScreenYmin + PushThreshold) {
        yVelocity = AccelConst;
    } else {
        yAcceleration = yVelocity * (PushBar / "1000" / (yPosition - ScreenYmin)) + AccelConst;
        yVelocity = yVelocity + yAcceleration;
    }
} else if (yPosition + "50" < yAvgPos) {
    yVelocity = yVelocity + (yAvgPos - yPosition) / "2500" * MaxVel;
} else if (yAvgPos < yPosition - "50") {
    yVelocity = yVelocity + (yAvgPos - yPosition) / "2500" * MaxVel;
} else {
    yAvgVel = eval("../:yAvgVel");
    if (yAvgVel * "1.1" < yVelocity) {
        yVelocity = yVelocity + "0.1" * yAvgVel;
    } else if (yVelocity < yAvgVel * "0.909090909090909") {
        yVelocity = yVelocity + "0.1" * yAvgVel;
    } else {
        yVelocity = yVelocity + "0.2" * (random("35") - "17");
    }
}
if ("0" < xVelocity and MaxVel < xVelocity) {
    xVelocity = MaxVel;
} else if (xVelocity < "0" and xVelocity < "0" - MaxVel) {
    xVelocity = "0" - MaxVel;
}
if ("0" < yVelocity and MaxVel < yVelocity) {
    yVelocity = MaxVel;
} else if (yVelocity < "0" and yVelocity < "0" - MaxVel) {
    yVelocity = "0" - MaxVel;
}
xMouse = getProperty("/MouseDrag", _x);
yMouse = getProperty("/MouseDrag", _y);
xMouse = xMouse - eval("/:DebugFGH");
yMouse = yMouse - eval("/:DebugFGV");
setProperty("../Target", _x, xMouse);
setProperty("../Target", _y, yMouse);
xDistance = xPosition - xMouse;
yDistance = yPosition - yMouse;
if ("0" < xDistance and xDistance < MouseRadius) {
    if ("0" < yDistance and yDistance < MouseRadius) {
        xVelocity = xVelocity + AccelConst * "2";
        yVelocity = yVelocity + AccelConst * "2";
    } else if (yDistance < "0" and "0" - MouseRadius < yDistance) {
        xVelocity = xVelocity + AccelConst * "2";
        yVelocity = yVelocity - AccelConst * "2";
    }
} else if (xDistance < "0" and "0" - MouseRadius < xDistance) {
    if ("0" < yDistance and yDistance < MouseRadius) {
        xVelocity = xVelocity - AccelConst * "2";
        yVelocity = yVelocity + AccelConst * "2";
    } else if (yDistance < "0" and "0" - MouseRadius < yDistance) {
        xVelocity = xVelocity - AccelConst * "2";
        yVelocity = yVelocity - AccelConst * "2";
    }
}
setProperty("../Fish" add WhichFish, _x, getProperty("../Fish" add WhichFish, _x) + xVelocity);
setProperty("../Fish" add WhichFish, _y, getProperty("../Fish" add WhichFish, _y) + yVelocity);
if (FacingRight eq "1" and xVelocity < "0") {
    setProperty("../Fish" add WhichFish, _xscale, getProperty("../Fish" add WhichFish, _xscale) * "-1");
    FacingRight = "0";
} else if (FacingRight eq "0" and "0" < xVelocity) {
    setProperty("../Fish" add WhichFish, _xscale, getProperty("../Fish" add WhichFish, _xscale) * "-1");
    FacingRight = "1";
}
if (xVelocity < "1" and "-1" < xVelocity) {
    if ("0" < yVelocity) {
        RotAngle = "90";
    } else {
        RotAngle = "-90";
    }
} else if ("1" < yVelocity) {
    if (yVelocity - xVelocity < "1" and "-1" < yVelocity - xVelocity) {
        RotAngle = "45";
    } else if (xVelocity < yVelocity) {
        RotAngle = "60";
    } else {
        RotAngle = "30";
    }
} else if (yVelocity - xVelocity < "1" and "-1" < yVelocity - xVelocity) {
    RotAngle = "-45";
} else if (yVelocity < "0" - xVelocity) {
    RotAngle = "-60";
} else {
    RotAngle = "-30";
}
if (FacingRight eq "0") {
    if ("0" < yVelocity) {
        BaseRotation = "90";
    } else {
        BaseRotation = "-90";
    }
} else {
    BaseRotation = "0";
}
Rotation = RotAngle + BaseRotation;
setProperty("../Fish" add WhichFish, _rotation, Rotation);
