#version 4
if (Wasntplaced eq "1") {
    GaKoro_Cog_Placed = eval("/:GaKoro_Cog_Placed");
    if (GaKoro_Cog_Placed eq "1") {
        setProperty("MainCog", _x, "-21.8");
        setProperty("MainCog", _y, "-7.2");
        Wasntplaced = "0";
    }
}
Dragging = eval("/Backpack/Backpack/:Dragging");
if ("-1" < Dragging) {
    xMouse = getProperty("/mousedrag", _x);
    yMouse = getProperty("/mousedrag", _y);
    if ("315" < xMouse and xMouse < "400") {
        if ("160" < yMouse and yMouse < "260") {
            CogCheck = eval("/Backpack/Backpack/:Slot" add Dragging);
            if (CogCheck eq "GaKoro-Cog") {
                tellTarget ("/") {
                    GaKoro_Cog_Placed = "1";
                    GaKoro_Cog = "0";
                }
                tellTarget ("SoundFX") {
                    gotoAndPlay("cogplaced");
                }
                setProperty("/Backpack/Backpack/GaKoro-Cog", _y, "-2000");
                tellTarget ("/Backpack/Backpack") {
                    Dragging = "-1";
                    gotoAndPlay("ResetOrder");
                }
            }
        }
    }
}
gotoAndPlay(2);
