#version 4
Chisel = eval("/:Chisel");
if (GiveItem eq "1") {
    if (Chisel eq "0") {
        segment = eval("/holder/textsub/:segment");
        if (Segment eq "4") {
            tellTarget ("/") {
                Chisel = "1";
            }
            tellTarget ("/Backpack/Backpack") {
                gotoAndPlay("ResetOrder");
            }
            GiveItem = "0";
        }
    }
}
