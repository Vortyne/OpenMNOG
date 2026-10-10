#version 4
xMouse = getProperty("/MouseDrag", _x);
yMouse = getProperty("/MouseDrag", _y);
xDistance = xMouse - xPohatu;
yDistance = yPohatu - yMouse;
Rotation = "90" * (xDistance / yDistance);
if ("25" < Rotation) {
    Rotation = "30";
    if (OldRotation1 eq Rotation) {
    } else {
        tellTarget ("Pohatu") {
            gotoAndPlay("Plus30");
        }
        if (AllowFire eq "1") {
            tellTarget ("Pohatu/Pohatu") {
                gotoAndStop("Ready");
                trace("Plus 30, swinging back now");
            }
        }
    }
} else if ("14" < Rotation and Rotation < "25") {
    Rotation = "20";
    if (OldRotation1 eq Rotation) {
    } else {
        tellTarget ("Pohatu") {
            gotoAndPlay("Plus20");
        }
        if (AllowFire eq "1") {
            tellTarget ("Pohatu/Pohatu") {
                gotoAndStop("Ready");
            }
        }
    }
} else if ("4" < Rotation and Rotation < "15") {
    Rotation = "10";
    if (OldRotation1 eq Rotation) {
    } else {
        tellTarget ("Pohatu") {
            gotoAndPlay("Plus10");
        }
        if (AllowFire eq "1") {
            tellTarget ("Pohatu/Pohatu") {
                gotoAndStop("Ready");
            }
        }
    }
} else if (Rotation < "5" and "-5" < Rotation) {
    Rotation = "0";
    if (OldRotation1 eq Rotation) {
    } else {
        tellTarget ("Pohatu") {
            gotoLabel("00");
            play();
        }
        if (AllowFire eq "1") {
            tellTarget ("Pohatu/Pohatu") {
                gotoAndStop("Ready");
            }
        }
    }
} else if ("-15" < Rotation and Rotation < "-4") {
    Rotation = "-10";
    if (OldRotation1 eq Rotation) {
    } else {
        tellTarget ("Pohatu") {
            gotoAndPlay("Minus10");
        }
        if (AllowFire eq "1") {
            tellTarget ("Pohatu/Pohatu") {
                gotoAndStop("Ready");
            }
        }
    }
} else if ("-25" < Rotation and Rotation < "-14") {
    Rotation = "-20";
    if (OldRotation1 eq Rotation) {
    } else {
        tellTarget ("Pohatu") {
            gotoAndPlay("Minus20");
        }
        if (AllowFire eq "1") {
            tellTarget ("Pohatu/Pohatu") {
                gotoAndStop("Ready");
            }
        }
    }
} else if (Rotation < "-30") {
    Rotation = "-30";
    if (OldRotation1 eq Rotation) {
    } else {
        tellTarget ("Pohatu") {
            gotoAndPlay("Minus30");
        }
        if (AllowFire eq "1") {
            tellTarget ("Pohatu/Pohatu") {
                gotoAndStop("Ready");
            }
        }
    }
}
if ("0" < yDistance) {
}
OldRotation5 = OldRotation4;
OldRotation4 = OldRotation3;
OldRotation3 = OldRotation2;
OldRotation2 = OldRotation1;
OldRotation1 = Rotation;
xPohatuOld5 = xPohatuOld4;
xPohatuOld4 = xPohatuOld3;
xPohatuOld3 = xPohatuOld2;
xPohatuOld2 = xPohatuOld1;
xPohatuOld1 = xPohatu;
if (xPohatu + "100" < xMouse) {
    xPohatu = getProperty("Pohatu", _x) + (xMouse - xPohatu) / "4";
} else if (xMouse < xPohatu - "100") {
    xPohatu = getProperty("Pohatu", _x) + (xMouse - xPohatu) / "4";
}
setProperty("Pohatu", _x, xPohatu);
tCounter = "1";
while (tCounter < "3") {
    if (eval("xPohatuOld" add tCounter) eq xPohatu) {
        setProperty("PohatuFade" add tCounter, _y, "-2000");
    } else {
        setProperty("PohatuFade" add tCounter, _x, eval("xPohatuOld" add tCounter));
        setProperty("PohatuFade" add tCounter, _y, yPohatu);
        if (eval("OldRotation" add tCounter)) {
        }
        if (eval("OldRotation" add tCounter) eq "30") {
            tellTarget ("PohatuFade" add tCounter) {
                gotoAndPlay("Plus30");
            }
        } else if (eval("OldRotation" add tCounter) eq "20") {
            tellTarget ("PohatuFade" add tCounter) {
                gotoAndPlay("Plus20");
            }
        } else if (eval("OldRotation" add tCounter) eq "10") {
            tellTarget ("PohatuFade" add tCounter) {
                gotoAndPlay("Plus10");
            }
        } else if (eval("OldRotation" add tCounter) eq "0") {
            tellTarget ("PohatuFade" add tCounter) {
                gotoLabel("00");
                play();
            }
        } else if (eval("OldRotation" add tCounter) eq "-10") {
            tellTarget ("PohatuFade" add tCounter) {
                gotoAndPlay("Minus10");
            }
        } else if (eval("OldRotation" add tCounter) eq "-20") {
            tellTarget ("PohatuFade" add tCounter) {
                gotoAndPlay("Minus20");
            }
        } else if (eval("OldRotation" add tCounter) eq "-30") {
            tellTarget ("PohatuFade" add tCounter) {
                gotoAndPlay("Minus30");
            }
        }
    }
    tCounter = tCounter + "1";
}
OldRotation5 = OldRotation4;
OldRotation4 = OldRotation3;
OldRotation3 = OldRotation2;
OldRotation2 = OldRotation1;
OldRotation1 = Rotation;
