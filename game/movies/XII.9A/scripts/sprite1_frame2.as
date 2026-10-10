#version 4
if (GiveItem eq "1") {
    section = eval("/holder/textsub/:section");
    if (Section eq "3") {
        tellTarget ("/") {
            LetterA = "1";
        }
        GiveItem = "0";
        tellTarget ("/Backpack/Backpack") {
            gotoAndPlay("ResetOrder");
        }
    }
}
