#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
if (eval("/:TaipuCanGo") eq "1") {
    TextToLoad = "Text_XII9B_Taipu_" add Language add ".txt";
} else {
    TextToLoad = "Text_XII9B_Taipu_2_" add Language add ".txt";
}
