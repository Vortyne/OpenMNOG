#version 4
PlayRollover1 = "0";
Language = eval("/:Language");
HadBook = eval("/:HadBook");
Book = eval("/:Book");
Chisel = eval("/:Chisel");
if (HadBook eq "0" and Book eq "0" and Chisel eq "1") {
    loadVariables("Text_V.4S_Fishermen_2_" add Language add ".txt", "FishermanBubble", "GET");
    loadVariables("Text_V.4S_Tohunga_2_" add Language add ".txt", "TohungaBubble", "GET");
} else {
    loadVariables("Text_V.4S_Fishermen_" add Language add ".txt", "FishermanBubble", "GET");
    loadVariables("Text_V.4S_Tohunga_" add Language add ".txt", "TohungaBubble", "GET");
}
tellTarget ("FishermanBubble") {
    BubbleNum = "1";
}
tellTarget ("TohungaBubble") {
    BubbleNum = "2";
}
