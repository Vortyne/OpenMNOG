#version 4
Dragging = eval("/Backpack/Backpack/:Dragging");
setProperty("MouseTrace", _x, xMouse - xOffset);
setProperty("MouseTrace", _y, yMouse - yOffset);
if ("-1" < Dragging) {
    KohliBallCheck = eval("/Backpack/Backpack/:Slot" add Dragging);
    if (KohliBallCheck eq "KohliBall") {
        xMouse = getProperty("/mousedrag", _x);
        yMouse = getProperty("/mousedrag", _y);
        xKeyHole = getProperty("../KeyHole", _x);
        yKeyHole = getProperty("../KeyHole", _y);
        xOffset = "0";
        yOffset = "0";
        if (xMouse - xOffset < xKeyHole + "100" and xKeyHole - "100" < xMouse - xOffset) {
            if (yMouse - yOffset < yKeyHole + "100" and yKeyHole - "100" < yMouse - yOffset) {
                tellTarget ("../") {
                    TextToLoad = "Text_VIII.2A_2_" add Language add ".txt";
                }
                loadMovie("TextSub.swf", "/holder/TextSub");
                Reload = "1";
                tellTarget ("/") {
                    KohliBallShown = "1";
                }
                tellTarget ("/Backpack/Backpack") {
                    Dragging = "-1";
                    gotoAndPlay("ResetOrder");
                }
            }
        }
    }
}
