#version 4
if ("-1" < Dragging) {
    setProperty(eval("Slot" add Dragging), _x, getProperty("/mousedrag", _x) + xOffset);
    setProperty(eval("Slot" add Dragging), _y, getProperty("/mousedrag", _y) + yOffset);
    DisplayText = eval("Slot" add Dragging add "_Description");
    if (eval("Slot" add Dragging) eq "Flute") {
        tellTarget ("../../") {
            FluteOpen = "1";
        }
    }
    if (eval("Slot" add Dragging) eq "lightstone") {
        tellTarget ("lightstone") {
            glow = "1";
        }
    } else {
        tellTarget ("lightstone") {
            glow = "0";
        }
    }
    if (eval("Slot" add Dragging) eq "FireStaff") {
        tellTarget ("FireStaff") {
            glow = "1";
        }
    } else {
        tellTarget ("FireStaff") {
            glow = "0";
        }
    }
} else {
    tellTarget ("lightstone") {
        glow = "0";
        gotoAndPlay("OFF");
    }
    tellTarget ("FireStaff") {
        glow = "0";
        gotoAndPlay("OFF");
    }
}
gotoAndPlay(2);
