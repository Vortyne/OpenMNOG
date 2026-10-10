#version 4
Dragging = eval("/Backpack/Backpack/:Dragging");
setProperty("MouseTrace", _x, xMouse - xOffset);
setProperty("MouseTrace", _y, yMouse - yOffset);
Brighten = "0";
if ("-1" < Dragging) {
    LightstoneCheck = eval("/Backpack/Backpack/:Slot" add Dragging);
    if (LightstoneCheck eq "lightstone") {
        xMouse = getProperty("/mousedrag", _x);
        yMouse = getProperty("/mousedrag", _y);
        xKeyHole = getProperty("Middle-Ground/Dark_FG", _x);
        yKeyHole = getProperty("Middle-Ground/Dark_FG", _y);
        xOffset = "0";
        yOffset = "0";
        if (xMouse - xOffset < xKeyHole + "700" and xKeyHole - "700" < xMouse - xOffset) {
            if (yMouse - yOffset < yKeyHole + "700" and yKeyHole - "700" < yMouse - yOffset) {
                Brighten = "1";
            }
        }
    }
}
