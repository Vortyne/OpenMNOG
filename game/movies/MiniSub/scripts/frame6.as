#version 4
if (TextIsLoaded eq "1") {
    InputText = AskMe;
    gotoAndPlay(7);
} else {
    gotoAndPlay(5);
}
