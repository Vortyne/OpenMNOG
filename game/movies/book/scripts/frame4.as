#version 4
if (ShiftTextUp eq "1") {
    if (CurrentTextSlot < MaxSlots) {
        if (ShiftCounter < TextHeight) {
            if (DoFadeout eq "1") {
                DoFadeout = "0";
                tellTarget ("Thumbnails/Thumbnails/Blank") {
                    gotoAndPlay("Fadeout");
                }
            }
            setProperty("TextSelect", _y, getProperty("TextSelect", _y) - ShiftStep);
            ShiftCounter = ShiftCounter + ShiftStep;
        } else {
            CurrentTextSlot = CurrentTextSlot + "1";
            ShiftDone = "1";
        }
    } else if (ShiftCounter < TextHeight) {
        if (DoFadeout eq "1") {
            DoFadeout = "0";
            tellTarget ("Thumbnails/Thumbnails/Blank") {
                gotoAndPlay("Fadeout");
            }
        }
        setProperty("TextSelect", _y, getProperty("TextSelect", _y) + (MaxSlots - 1) * ShiftStep);
        ShiftCounter = ShiftCounter + ShiftStep;
    } else {
        CurrentTextSlot = "1";
        ShiftDone = "1";
    }
} else if (ShiftTextDown eq "1") {
    if ("1" < CurrentTextSlot) {
        if (ShiftCounter < TextHeight) {
            if (DoFadeout eq "1") {
                DoFadeout = "0";
                tellTarget ("Thumbnails/Thumbnails/Blank") {
                    gotoAndPlay("Fadeout");
                }
            }
            setProperty("TextSelect", _y, getProperty("TextSelect", _y) + ShiftStep);
            ShiftCounter = ShiftCounter + ShiftStep;
        } else {
            CurrentTextSlot = CurrentTextSlot - "1";
            ShiftDone = "1";
        }
    } else if (ShiftCounter < TextHeight) {
        if (DoFadeout eq "1") {
            DoFadeout = "0";
            tellTarget ("Thumbnails/Thumbnails/Blank") {
                gotoAndPlay("Fadeout");
            }
        }
        setProperty("TextSelect", _y, getProperty("TextSelect", _y) - (MaxSlots - 1) * ShiftStep);
        ShiftCounter = ShiftCounter + ShiftStep;
    } else {
        CurrentTextSlot = MaxSlots;
        ShiftDone = "1";
    }
}
// A slot reached: the next queued click goes on at once (the picture stays covered); with none, it fades in.
if (ShiftDone eq "1") {
    ShiftDone = "0";
    ShiftCounter = "0";
    ShiftTextUp = "0";
    ShiftTextDown = "0";
    if ("0" < QueuedUp) {
        QueuedUp = QueuedUp - "1";
        ShiftTextUp = "1";
    } else if ("0" < QueuedDown) {
        QueuedDown = QueuedDown - "1";
        ShiftTextDown = "1";
    } else {
        tellTarget ("Thumbnails") {
            DoFadein = "1";
        }
        Shifting = "0";
    }
}
