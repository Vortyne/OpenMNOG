#version 4
Dragging = eval("/Backpack/Backpack/:Dragging");
setProperty("MouseTrace", _x, xMouse - xOffset);
setProperty("MouseTrace", _y, yMouse - yOffset);
if ("-1" < Dragging) {
    KohliBallCheck = eval("/Backpack/Backpack/:Slot" add Dragging);
    if (KohliBallCheck eq "KohliBall") {
        xMouse = getProperty("/mousedrag", _x);
        yMouse = getProperty("/mousedrag", _y);
        xKeyHole = getProperty("KeyHole", _x);
        yKeyHole = getProperty("KeyHole", _y);
        xOffset = eval("/:DebugMGH");
        yOffset = eval("/:DebugMGV");
        if (xMouse - xOffset < xKeyHole + "50" and xKeyHole - "50" < xMouse - xOffset) {
            if (yMouse - yOffset < yKeyHole + "50" and yKeyHole - "50" < yMouse - yOffset) {
                tellTarget ("../") {
                    TextToLoad = "Text_VIII.3A_2_" add Language add ".txt";
                }
                loadMovie("TextSub.swf", "/holder/TextSub");
                Reload = "1";
                tellTarget ("/") {
                    KohliBall = "0";
                    KohliBallUsed = "1";
                }
                setProperty("/Backpack/Backpack/KohliBall", _y, "-2000");
                tellTarget ("/Backpack/Backpack") {
                    Dragging = "-1";
                    gotoAndPlay("ResetOrder");
                }
            }
        }
    }
}
