#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
if (eval("/:cKapura") eq "1") {
    TextToLoad = "Text_XIX.5A_Kopeke_" add Language add ".txt";
} else {
    TextToLoad = "Text_XIX.5A_1_" add Language add ".txt";
}
