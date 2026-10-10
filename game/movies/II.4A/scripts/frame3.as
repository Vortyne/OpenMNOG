#version 4
currenttime = eval("/:currenttime");
TimeOfLaunch = "1004122800";
RealTime = ServerBasedDate + currenttime / "1000";
CountDownTime = TimeOfLaunch - RealTime;
MHours = int(CountDownTime / "10000");
MMinutes = int(CountDownTime / "100") - MHours * "100";
MSeconds = int(CountDownTime) - MMinutes * "100" - MHours * "10000";
CountDown = MHours add "." add MMinutes add "." add MSeconds;
TextRotation = getProperty("/holder/Sky", _rotation);
TextHeight = getProperty("/holder/Sky", _y);
TimeSpent = currenttime - Time_Start;
if (getProperty("/Mousedrag", _x) < "172.5") {
    if (RotateIsOn eq "0" and ShiftIsOn eq "0") {
        if ("2000" < currenttime - Time_Start) {
            tellTarget ("/holder/SFX-holder") {
                gotoAndPlay("Telescope_ON");
            }
            Time_Start = currenttime;
        }
    }
    RotateIsOn = "1";
    setProperty("/holder/Sky", _rotation, getProperty("/holder/Sky", _rotation) - "2");
    setProperty("/holder/Sky", _xscale, "100");
    setProperty("/holder/Sky", _yscale, "100");
} else if ("597.5" < getProperty("/Mousedrag", _x)) {
    if (RotateIsOn eq "0" and ShiftIsOn eq "0") {
        if ("2000" < currenttime - Time_Start) {
            tellTarget ("/holder/SFX-holder") {
                gotoAndPlay("Telescope_ON");
            }
            Time_Start = currenttime;
        }
    }
    RotateIsOn = "1";
    setProperty("/holder/Sky", _rotation, getProperty("/holder/Sky", _rotation) + "2");
    setProperty("/holder/Sky", _xscale, "100");
    setProperty("/holder/Sky", _yscale, "100");
} else {
    if (RotateIsOn eq "1" and ShiftIsOn eq "0") {
        tellTarget ("/holder/SFX-holder") {
            gotoAndPlay("Telescope_OFF");
        }
    }
    RotateIsOn = "0";
}
if (getProperty("/Mousedrag", _y) < "100") {
    if (getProperty("/holder/Sky", _y) < "212.5") {
        if (RotateIsOn eq "0" and ShiftIsOn eq "0") {
            if ("2000" < currenttime - Time_Start) {
                tellTarget ("/holder/SFX-holder") {
                    gotoAndPlay("Telescope_ON");
                }
                Time_Start = currenttime;
            }
        }
        ShiftIsOn = "1";
        setProperty("/holder/Sky", _y, getProperty("/holder/Sky", _y) + "10");
    } else {
        if (RotateIsOn eq "0" and ShiftIsOn eq "1") {
            tellTarget ("/holder/SFX-holder") {
                gotoAndPlay("Telescope_OFF");
            }
        }
        ShiftIsOn = "0";
    }
} else if ("325" < getProperty("/Mousedrag", _y)) {
    if ("-500" < getProperty("/holder/Sky", _y)) {
        if (RotateIsOn eq "0" and ShiftIsOn eq "0") {
            if ("2000" < currenttime - Time_Start) {
                tellTarget ("/holder/SFX-holder") {
                    gotoAndPlay("Telescope_ON");
                }
                Time_Start = currenttime;
            }
        }
        ShiftIsOn = "1";
        setProperty("/holder/Sky", _y, getProperty("/holder/Sky", _y) - "10");
    } else {
        if (RotateIsOn eq "0" and ShiftIsOn eq "1") {
            tellTarget ("/holder/SFX-holder") {
                gotoAndPlay("Telescope_OFF");
            }
        }
        ShiftIsOn = "0";
    }
} else {
    if (RotateIsOn eq "0" and ShiftIsOn eq "1") {
        tellTarget ("/holder/SFX-holder") {
            gotoAndPlay("Telescope_OFF");
        }
    }
    ShiftIsOn = "0";
}
