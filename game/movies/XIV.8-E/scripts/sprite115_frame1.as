#version 4
Language = eval("/:Language");
TextToLoad = "Text_IV6_ShrineSpeech_" add Language add ".txt";
loadVariables(TextToLoad, "ShrineSpeech");
stop();
