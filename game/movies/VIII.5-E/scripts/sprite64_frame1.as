#version 4
tellTarget ("Astrologer") {
    gotoAndPlay("idle");
}
tellTarget ("/holder/Middle-Ground/Light2/Lightfish") {
    gotoAndPlay(30);
}
Language = eval("/:Language");
loadVariables("Text_VIII.5E_Huki_1_" add Language add ".txt", "HukiBubble");
