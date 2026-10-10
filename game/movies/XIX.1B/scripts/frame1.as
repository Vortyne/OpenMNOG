#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
TextToLoad = "Text_XIX.1B_1_" add Language add ".txt";
tellTarget ("/") {
    GondolaFrom = "Ko-Koro";
}
