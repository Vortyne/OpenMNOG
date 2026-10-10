#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
if (eval("/:LostGame") eq "1") {
    TextToLoad = "Text_XVIII.5A_2_" add Language add ".txt";
} else {
    TextToLoad = "Text_XVIII.5A_1_" add Language add ".txt";
}
