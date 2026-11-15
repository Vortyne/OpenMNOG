#version 4
Gnomon = eval("/:Gnomon");
LetterA = eval("/:LetterA");
// (OpenMNOG: also once the gnomon is on the sundial, Gnomon 2; before, he went back to his first talk and gave
// another letter.)
if (Gnomon eq "1" or Gnomon eq "2" or LetterA eq "1") {
    GiveItem = "0";
} else {
    GiveItem = "1";
}
