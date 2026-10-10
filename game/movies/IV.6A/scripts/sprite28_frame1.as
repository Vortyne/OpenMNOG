#version 4
Dragging = eval("/Backpack/Backpack/:Dragging");
setProperty("MouseTrace", _x, xMouse - xOffset);
setProperty("MouseTrace", _y, yMouse - yOffset);
if ("-1" < Dragging) {
    LetterBcheck = eval("/Backpack/Backpack/:Slot" add Dragging);
    if (LetterBcheck eq "LetterB") {
        xMouse = getProperty("/mousedrag", _x);
        yMouse = getProperty("/mousedrag", _y);
        xKeyHole = getProperty("../KeyHole", _x);
        yKeyHole = getProperty("../KeyHole", _y);
        xOffset = "0";
        yOffset = "0";
        if (xMouse - xOffset < xKeyHole + "100" and xKeyHole - "100" < xMouse - xOffset) {
            if (yMouse - yOffset < yKeyHole + "100" and yKeyHole - "100" < yMouse - yOffset) {
                tellTarget ("../") {
                    TextToLoad = "Text_IV.6A_3_" add Language add ".txt";
                }
                tellTarget ("/") {
                    fTaKoro = "10";
                }
                loadMovie("TextSub.swf", "/holder/TextSub");
                Reload = "1";
                tellTarget ("/Backpack/Backpack") {
                    Dragging = "-1";
                    gotoAndPlay("ResetOrder");
                }
            }
        }
    }
}
