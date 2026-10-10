#version 4
Dragging = eval("/Backpack/Backpack/:Dragging");
if ("-1" < Dragging) {
    LavaBoardCheck = eval("/Backpack/Backpack/:Slot" add Dragging);
    if (LavaBoardCheck eq "LavaBoard") {
        xMouse = getProperty("/mousedrag", _x);
        yMouse = getProperty("/mousedrag", _y);
        xKeyHole = getProperty("../KeyHole", _x);
        yKeyHole = getProperty("../KeyHole", _y);
        xOffset = eval("/:DebugMGH");
        yOffset = eval("/:DebugMGV");
        if (xMouse - xOffset < xKeyHole + "150" and xKeyHole - "150" < xMouse - xOffset) {
            if (yMouse - yOffset < yKeyHole + "100" and yKeyHole - "100" < yMouse - yOffset) {
                tellTarget ("/") {
                    MovieName = "Episode_5-1.swf";
                    ShiftOff = "1";
                    Destination = "XV.3-S.swf";
                    BeginLoadMovie = "1";
                }
            }
        }
    }
}
