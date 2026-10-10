#version 4
stopDrag();
startDrag("/mousedrag", "1");
tellTarget ("/") {
    ShiftOff = "0";
}
tellTarget ("../") {
    Holding = "0";
}
if (PlacedRock eq "1") {
} else if (PickedUp eq "1") {
    xRock = getProperty(eval("../Rock" add WhichChecking), _x);
    yRock = getProperty(eval("../Rock" add WhichChecking), _y);
    xBasket = getProperty("../BasketFront", _x);
    yBasket = getProperty("../BasketFront", _y);
    if (xRock < xBasket + "17" and xBasket - "17" < xRock) {
        if (yRock < yBasket - "40") {
            Placed = "1";
            set("../:WhichBeingPlaced", WhichRock);
            tellTarget ("../") {
                duplicateMovieClip("Rock" add WhichBeingPlaced, "PlacedRock" add WhichBeingPlaced, "16384" + ("111" + WhichBeingPlaced));
                setProperty("Rock" add WhichBeingPlaced, _x, "2000");
                set("PlacedRock" add WhichBeingPlaced, "1");
            }
            tellTarget ("../PlacedRock" add WhichRock) {
                WhichRock = eval("../:WhichBeingPlaced");
                Whichone = "Placed rock";
                PlacedRock = "1";
            }
        }
    }
    tellTarget ("../Rock" add WhichRock) {
        PickedUp = "0";
        Whichone = "Regular rock";
    }
}
