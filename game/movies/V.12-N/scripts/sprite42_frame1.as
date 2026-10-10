#version 4
PlayRollover1 = "0";
Language = eval("/:Language");
HadBook = eval("/:HadBook");
if (HadBook eq "0") {
    loadVariables("Text_V.4N_Guard_2_" add Language add ".txt", "GuardBubble");
} else {
    loadVariables("Text_V.4N_Guard_" add Language add ".txt", "GuardBubble");
}
