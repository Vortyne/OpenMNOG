#version 4
if (TextIsLoaded eq "1") {
    if (OpenIsDone eq "1") {
        InputText = AskMe;
        OpenQBox = "1";
    } else {
        InputText = OpenStatement1;
        CurrentSegments = OpenStatementNum;
    }
    gotoAndPlay(7);
} else {
    loadcounter = loadcounter + "1";
    if ("500" < loadcounter) {
        loadVariables(TextToLoad, "/holder/TextSub");
        loadcounter = "0";
    }
    gotoAndPlay(5);
}
