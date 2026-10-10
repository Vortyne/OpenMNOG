#version 4
Dragging = eval("/Backpack/Backpack/:Dragging");
setProperty("MouseTrace", _x, xMouse - xOffset);
setProperty("MouseTrace", _y, yMouse - yOffset);
if ("-1" < Dragging) {
    QuarryKeyCheck = eval("/Backpack/Backpack/:Slot" add Dragging);
    if (QuarryKeyCheck eq "QuarryKey") {
        if (eval("/:QuarryKey") eq "1") {
            xMouse = getProperty("/mousedrag", _x);
            yMouse = getProperty("/mousedrag", _y);
            xKeyHole = getProperty("KeyHole", _x);
            yKeyHole = getProperty("KeyHole", _y);
            xOffset = eval("/:DebugMGH");
            yOffset = eval("/:DebugMGV");
            if (xMouse - xOffset < xKeyHole + "50" and xKeyHole - "50" < xMouse - xOffset) {
                if (yMouse - yOffset < yKeyHole + "50" and yKeyHole - "50" < yMouse - yOffset) {
                    gotoAndPlay(3);
                    tellTarget ("/") {
                        QuarryKey = "0";
                        QuarryKeyUsed = "1";
                    }
                    setProperty("/Backpack/Backpack/QuarryKey", _y, "-2000");
                    tellTarget ("/Backpack/Backpack") {
                        Dragging = "-1";
                        gotoAndPlay("ResetOrder");
                    }
                }
            }
        }
    }
}
