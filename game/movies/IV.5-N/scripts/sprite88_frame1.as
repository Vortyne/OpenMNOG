#version 4
Language = eval("/:Language");
GaKoro = eval("/:GaKoro");
TextToLoad = "Text_IV6_FarmerSpeech_" add Language add ".txt";
loadVariables(TextToLoad, "FarmerSpeech", "GET");
stop();
if (GaKoro eq "0") {
    gotoAndPlay(2);
}
