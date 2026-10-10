#version 4
if (eval("/:OnuKoroDoor") eq "1") {
    Language = eval("/:Language");
    loadVariables("Text_XII.9E_Prospector_" add Language add ".txt", "ProspectorBubble");
} else {
    setProperty("ProspectorBubble", _y, "3000");
}
