#version 4
Stopped = "0";
Language = eval("/:Language");
if (eval("/:LavaBoard") < "1" and eval("/:Backpack") eq "1") {
    TextToLoad = "Text_IV6_Surfer_1_" add Language add ".txt";
} else {
    TextToLoad = "Text_IV6_Surfer_2_" add Language add ".txt";
}
loadVariables(TextToLoad, "Bubble", "GET");
