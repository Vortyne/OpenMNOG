#version 4
Book = eval("/:Book");
if (GiveItem eq "1") {
    Dragging = eval("/Backpack/Backpack/:Dragging");
    if ("-1" < Dragging) {
        ChiselCheck = eval("/Backpack/Backpack/:Slot" add Dragging);
        if (ChiselCheck eq "Chisel") {
            xMouse = getProperty("/mousedrag", _x);
            yMouse = getProperty("/mousedrag", _y);
            xKeyHole = getProperty("../KeyHole", _x);
            yKeyHole = getProperty("../KeyHole", _y);
            xOffset = eval("/:DebugMGH");
            yOffset = eval("/:DebugMGV");
            if (xMouse - xOffset < xKeyHole + "100" and xKeyHole - "100" < xMouse - xOffset) {
                if (yMouse - yOffset < yKeyHole + "100" and yKeyHole - "100" < yMouse - yOffset) {
                    GiveItem = "0";
                    Reload = "1";
                    tellTarget ("../../") {
                        TextToLoad = "Text_V.8A_4_" add Language add ".txt";
                    }
                    loadMovie("TextSub.swf", "/holder/TextSub");
                    tellTarget ("/") {
                        Book = "1";
                    }
                    tellTarget ("/Backpack/Backpack") {
                        Dragging = "-1";
                        gotoAndPlay("ResetOrder");
                    }
                }
            }
        }
    }
}
