#version 4
if (frameLoaded(847)) {
    Text1 = "Loading, please wait!";
    Language = eval("/:Language");
    loadVariables("Text_Episode_4-3_" add Language add ".txt", "/holder");
    TextCount = "1";
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    gotoAndPlay(5);
}
