#version 4
if (Placed eq "0" and PlacedRock != "1") {
    set("../:WhichChecking", WhichRock);
    startDrag("../Rock" add WhichRock, "0");
    tellTarget ("/") {
        ShiftOff = "1";
    }
    PickedUp = "1";
    tellTarget ("../") {
        Holding = "1";
    }
} else if (PlacedRock eq "1") {
    set("../:WhichChecking", WhichRock);
    PickedUp = "1";
    DonePlacing = eval("../:DonePlacing" add WhichRock);
    if (DonePlacing eq "1") {
        setProperty("../PlacedRock" add WhichRock, _x, "2000");
        startDrag("../Rock" add WhichRock, "1");
        tellTarget ("/") {
            ShiftOff = "1";
        }
        tellTarget ("../Rock" add WhichRock) {
            PickedUp = "1";
            Placed = "0";
        }
        tellTarget ("../") {
            NumPlaced = NumPlaced - "1";
            Holding = "1";
            set("DonePlacing" add WhichChecking, "0");
            set("PlacedRock" add WhichChecking, "0");
        }
    }
}
