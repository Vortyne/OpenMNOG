#version 4
if (reload eq "1") {
    reload = "0";
    tellTarget ("../") {
        TextToLoad = "Text_IV6_Surfer_2_" add Language add ".txt";
        loadVariables(TextToLoad, "Bubble");
    }
}
