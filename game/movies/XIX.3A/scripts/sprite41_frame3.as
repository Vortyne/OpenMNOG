#version 4
if (Frozen eq "1") {
    Dragging = eval("/Backpack/Backpack/:Dragging");
    if ("-1" < Dragging) {
        FireStaffCheck = eval("/Backpack/Backpack/:Slot" add Dragging);
        if (FireStaffCheck eq "FireStaff") {
            xMouse = getProperty("/mousedrag", _x);
            yMouse = getProperty("/mousedrag", _y);
            xKeyHole = getProperty("../KeyHole", _x);
            yKeyHole = getProperty("../KeyHole", _y);
            xOffset = "0";
            yOffset = "0";
            if (xMouse - xOffset < xKeyHole + "100" and xKeyHole - "100" < xMouse - xOffset) {
                if (yMouse - yOffset < yKeyHole + "100" and yKeyHole - "100" < yMouse - yOffset) {
                    Frozen = "0";
                    tellTarget ("../Kopeke") {
                        gotoAndPlay("Play");
                    }
                    setProperty("/Holder/TurnArounds", _y, "-1000");
                }
            }
        }
    }
}
