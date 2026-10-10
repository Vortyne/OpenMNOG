#version 4
loadMovie("sky2.swf", "/holder/Back-Ground/Sky-holder");
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
TextToLoad = "Text_XI.1A_1_" add Language add ".txt";
tellTarget ("/") {
    CrabFrom = "OnuKoro";
}
