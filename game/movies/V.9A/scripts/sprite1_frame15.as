#version 4
if (GiveItem eq "1") {
    segment = eval("/holder/textsub/:segment");
    if (Segment eq "3") {
        tellTarget ("/") {
            Gnomon = "1";
        }
        tellTarget ("/Backpack/Backpack") {
            gotoAndPlay("ResetOrder");
        }
        GiveItem = "0";
    }
}
