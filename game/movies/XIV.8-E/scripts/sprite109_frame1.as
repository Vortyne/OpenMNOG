#version 4
Language = eval("/:Language");
TextToLoad = "Text_IV6_FarmerSpeech_" add Language add ".txt";
loadVariables(TextToLoad, "FarmerSpeech");
stop();
