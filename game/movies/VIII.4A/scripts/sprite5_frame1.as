#version 4
Segment = eval("../../../TextSub/:Section");
if (Segment eq "4") {
    tellTarget ("../") {
        gotoAndPlay("Sneeze");
    }
    gotoAndStop(3);
}
