#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
if ("6" < eval("/:KoKoro")) {
    TextToLoad = "Text_XX.4A_2_" add Language add ".txt";
} else {
    TextToLoad = "Text_XX.4A_1_" add Language add ".txt";
}
