#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Language = eval("/:Language");
if (eval("/:Taipu") eq "XVII.1-N") {
    TextToLoad = "Text_XVII.1A_2_" add Language add ".txt";
} else {
    TextToLoad = "Text_XVII.1A_1_" add Language add ".txt";
}
