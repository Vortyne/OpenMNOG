#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Language = eval("/:Language");
OnuKoroLava = eval("/:OnuKoroLava");
if (OnuKoroLava eq "1") {
    TextToLoad = "Text_X.6B_2_" add Language add ".txt";
} else {
    TextToLoad = "Text_X.6B_1_" add Language add ".txt";
}
