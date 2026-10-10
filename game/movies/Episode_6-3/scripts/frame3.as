#version 4
if (frameLoaded(741)) {
    Text1 = "Loading, please wait!";
    Language = eval("/:Language");
    loadVariables("Text_Episode_6-3_" add Language add ".txt", "/holder");
    textcount = "1";
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    gotoAndPlay(5);
}
