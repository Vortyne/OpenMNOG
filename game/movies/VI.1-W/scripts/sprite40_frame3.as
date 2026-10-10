#version 4
Dragging = eval("/Backpack/Backpack/:Dragging");
FindGakoro_Cog = eval("../:FindGakoro_Cog");
xOffset = eval("/:DebugMGH");
yOffset = eval("/:DebugMGV");
if ("-1" < Dragging) {
    xMouse = getProperty("/mousedrag", _x) - xOffset;
    yMouse = getProperty("/mousedrag", _y) - yOffset;
    xCogPlacement = getProperty(eval("../CogPlacement" add FindGakoro_Cog), _x);
    yCogPlacement = getProperty(eval("../CogPlacement" add FindGakoro_Cog), _y);
    DragCheck = eval("/Backpack/Backpack/:Slot" add Dragging);
    if (xCogPlacement - "75" < xMouse and xMouse < xCogPlacement + "75") {
        if (yCogPlacement - "75" < yMouse and yMouse < yCogPlacement + "75") {
            if (DragCheck eq "lightstone" and AllowAnimation eq "1") {
                tellTarget ("CogAnim") {
                    gotoAndPlay("Glint");
                }
            }
        }
    }
}
gotoAndPlay(2);
