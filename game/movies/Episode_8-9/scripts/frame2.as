#version 4
Text1 = "Loading, please wait!";
Language = eval("/:Language");
loadVariables("Text_Episode_8-9_" add Language add ".txt", "/holder");
textcount = "1";
tellTarget ("/") {
    Movieisloaded = "1";
}
