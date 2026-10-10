#version 4
Language = eval("/:Language");
if (eval("/:PoKoro") eq "1") {
    loadVariables("Text_V.10_2_Maku_" add Language add ".txt", "MakuBubble", "GET");
} else {
    loadVariables("Text_V.10_Maku_" add Language add ".txt", "MakuBubble", "GET");
}
