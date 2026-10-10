#version 4
Language = eval("/:Language");
TextToLoad = "Text_XXIII.5-N2_" add Language add ".txt";
loadVariables(TextToLoad, "Kapura");
loadVariables(TextToLoad, "Maku");
loadVariables(TextToLoad, "Kongu");
loadVariables(TextToLoad, "Tamaru");
loadVariables(TextToLoad, "Jala");
stop();
