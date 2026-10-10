#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
TextToLoad = "Text_IV.8A_1_" add Language add ".txt";
tellTarget ("/") {
    GondolaFrom = "Ta-Koro";
}
