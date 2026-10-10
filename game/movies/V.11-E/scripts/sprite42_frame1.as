#version 4
tellTarget ("Astrologer") {
    gotoAndPlay("idle");
}
tellTarget ("/holder/Middle-Ground/Light2/Lightfish") {
    gotoAndPlay(30);
}
Language = eval("/:Language");
loadVariables("Text_V.11E_Guard_" add Language add ".txt", "GuardBubble");
