#version 4
Gnomon = eval("/:Gnomon");
LetterA = eval("/:LetterA");
if (Gnomon eq "1" or LetterA eq "1") {
    GiveItem = "0";
} else {
    GiveItem = "1";
}
