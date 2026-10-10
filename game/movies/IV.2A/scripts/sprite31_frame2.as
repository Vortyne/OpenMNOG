#version 4
if (AllowGive eq "1") {
    section = eval("/holder/textsub/:section");
    segment = eval("/holder/textsub/:segment");
    if (section eq "3" and segment eq "5") {
        tellTarget ("/") {
            Ensign = "1";
        }
        tellTarget ("/Backpack/Backpack") {
            gotoAndPlay("ResetOrder");
        }
        AllowGive = "0";
    }
}
