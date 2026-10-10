#version 4
Section = eval("../TextSub/:Section");
Segment = eval("../TextSub/:Segment");
if (Section eq "3" and Segment eq "2") {
    ShowDoor = "1";
} else {
    ShowDoor = "0";
    DoorShown = "0";
}
if (ShowDoor eq "1" and DoorShown eq "0") {
    DoorShown = "1";
    setTarget("../Tohunga");
    if (eval("/:KoKoro") < "6") {
        gotoAndPlay("Open");
    } else if (eval("/:KoKoro") eq "6") {
        gotoAndPlay("Point");
    } else if (eval("/:KoKoro") eq "7") {
        gotoAndPlay("Point2");
    }
    setTarget("");
}
