#version 4
if ("0" < CheckDrag) {
    if (eval("/:Gnomon") eq "2") {
        Dragging = eval("/Backpack/Backpack/:Dragging");
        if ("0" < Dragging) {
            item = eval("/Backpack/Backpack/:Slot" add Dragging);
            if (item eq "lightstone") {
                setProperty("Rock1/Shadow", _visible, "1");
            }
        } else {
            setProperty("Rock1/Shadow", _visible, "0");
        }
    } else {
        setProperty("Rock1/Shadow", _visible, "0");
        setProperty("Rock1", _visible, "0");
    }
}
if (getProperty("Rock1/Shadow", _visible) eq "1") {
    CurrentFrame = eval("Rock1/Shadow/:JumptoFrame");
    if (CurrentFrame < TargetFrame + TargetDeviation and TargetFrame - TargetDeviation < CurrentFrame) {
        if (Timing eq "1") {
            EndTime = getTimer();
            if (HoldTime < EndTime - StartTime) {
                gotoAndPlay(8);
            }
        } else {
            Timing = "1";
            StartTime = getTimer();
            tellTarget ("Base/glow") {
                gotoAndPlay("glow");
            }
        }
    } else {
        Timing = "0";
        tellTarget ("Base/glow") {
            gotoAndPlay("idle");
        }
    }
}
