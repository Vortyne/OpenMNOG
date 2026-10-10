#version 4
PlayRollover1 = "0";
stop();
Language = eval("/:Language");
GaKoro = eval("/:GaKoro");
if (GaKoro eq "0") {
    TextToLoad = "Text_XIV.8W_Guard_" add Language add ".txt";
    loadVariables(TextToLoad, "GuardBubble", "GET");
    gotoAndPlay(2);
}
