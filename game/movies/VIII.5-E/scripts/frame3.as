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
        xKeyHole = getProperty("Darkness", _x);
        yKeyHole = getProperty("Darkness", _y);
        xOffset = "0";
        yOffset = "0";
        if (xMouse - xOffset < xKeyHole + "200" and xKeyHole - "200" < xMouse - xOffset) {
            if (yMouse - yOffset < yKeyHole + "200" and yKeyHole - "200" < yMouse - yOffset) {
                Brighten = "1";
            }
        }
    }
}
