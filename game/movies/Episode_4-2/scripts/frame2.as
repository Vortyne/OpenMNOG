#version 4
if (frameLoaded(683)) {
    Text1 = "Loading, please wait!";
    Language = eval("/:Language");
    loadVariables("Text_Episode_4-2_" add Language add ".txt", "/holder");
    SecondOne = "1";
    tellTarget ("/") {
        Movieisloaded = "1";
    }
    gotoAndPlay(5);
}
