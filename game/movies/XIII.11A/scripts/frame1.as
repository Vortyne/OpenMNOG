#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Language = eval("/:Language");
if (eval("/:OnukoroLava") eq "1") {
    TextToLoad = "Text_XIII11A_2_" add Language add ".txt";
} else {
    TextToLoad = "Text_XIII11A_1_" add Language add ".txt";
}
