#version 4
tellTarget ("/") {
    Startedloading = "1";
}
VIII4NVisited = eval("/:VIII4NVisited");
if (VIII4NVisited < "1") {
    tellTarget ("/") {
        VIII4NVisited = "1";
    }
}
Count = "0";
Language = eval("/:Language");
TextToLoad = "Text_VIII.5A_1_" add Language add ".txt";
