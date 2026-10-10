#version 4
if (Ensign eq "1") {
    Dragging = eval("/Backpack/Backpack/:Dragging");
    if ("-1" < Dragging) {
        EnsignCheck = eval("/Backpack/Backpack/:Slot" add Dragging);
        if (EnsignCheck eq "Ensign") {
            xMouse = getProperty("/mousedrag", _x);
            yMouse = getProperty("/mousedrag", _y);
            xKeyHole = getProperty("../Middle-Ground/KeyHole", _x);
            yKeyHole = getProperty("../Middle-Ground/KeyHole", _y);
            xOffset = getProperty("../Middle-Ground", _x);
            yOffset = getProperty("../Middle-Ground", _y);
            if (xMouse - xOffset < xKeyHole + "100" and xKeyHole - "100" < xMouse - xOffset) {
                if (yMouse - yOffset < yKeyHole + "100" and yKeyHole - "100" < yMouse - yOffset) {
                    tellTarget ("../Middle-Ground") {
                        TextToLoad = "Text_IV8_Guard_2_" add Language add ".txt";
                        loadVariables(TextToLoad, "Bubble1");
                    }
                    Reload = "1";
                    tellTarget ("/Backpack/Backpack") {
                        Dragging = "-1";
                        gotoAndPlay("ResetOrder");
                    }
                    set("../:EnsignPass", "1");
                    // (Port: remembered for good, the cable car stays open to you.)
                    tellTarget ("/") {
                        EnsignPass = "1";
                    }
                    if (eval("../Middle-Ground/Guard/:WalkClick") eq "0") {
                        tellTarget ("../Middle-Ground/Guard") {
                            WalkClick = "1";
                        }
                    } else {
                        tellTarget ("../Middle-Ground/Bubble1") {
                            Num = "1";
                            Input = eval("Statement_" add Num);
                        }
                    }
                }
            }
        }
    }
}
tellTarget ("../Middle-Ground/Bubble1") {
    Num = "1";
    Input = eval("Statement_" add Num);
}
