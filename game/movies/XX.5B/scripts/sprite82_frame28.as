#version 4
segment = eval("/holder/textsub/:segment");
if (segment eq "2" and MapGuess eq "1") {
    tellTarget ("/") {
        LetterB = "1";
    }
    tellTarget ("/Backpack/Backpack") {
        gotoAndPlay("ResetOrder");
    }
    Mapguess = "0";
}
