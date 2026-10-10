#version 4
Book = eval("/:Book");
if (Book eq "1" and GiveBook eq "1") {
    GiveBook = "0";
    tellTarget ("/") {
        Book = "1";
        HadBook = "1";
    }
    tellTarget ("/Backpack/Backpack") {
        gotoAndPlay("ResetOrder");
    }
}
segment = eval("/holder/textsub/:segment");
if (Segment eq "5" and GiveBook eq "1") {
    tellTarget ("/") {
        Book = "1";
    }
}
