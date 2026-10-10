#version 4
if (eval("/:Gnomon") eq "1") {
    Dragging = eval("/Backpack/Backpack/:Dragging");
    setProperty("MouseTrace", _x, xMouse - xOffset);
    setProperty("MouseTrace", _y, yMouse - yOffset);
    if ("-1" < Dragging) {
        itemcheck = eval("/Backpack/Backpack/:Slot" add Dragging);
        if (itemcheck eq "gnomon") {
            xMouse = getProperty("/mousedrag", _x);
            yMouse = getProperty("/mousedrag", _y);
            xKeyHole = getProperty("../KeyHole", _x);
            yKeyHole = getProperty("../KeyHole", _y);
            xOffset = "0";
            yOffset = "0";
            if (xMouse - xOffset < xKeyHole + "100" and xKeyHole - "100" < xMouse - xOffset) {
                if (yMouse - yOffset < yKeyHole + "100" and yKeyHole - "100" < yMouse - yOffset) {
                    Reload = "1";
                    tellTarget ("/") {
                        Gnomon = "2";
                    }
                    setProperty("../Rock1", _visible, "1");
                    setProperty("/Backpack/Backpack/Gnomon", _y, "-2000");
                    tellTarget ("/Backpack/Backpack") {
                        Dragging = "-1";
                        gotoAndPlay("ResetOrder");
                    }
                }
            }
        }
    }
}
