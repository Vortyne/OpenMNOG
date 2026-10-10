#version 4
if (TextCount < "7") {
    Text = eval("Text" add TextCount);
    TextCount = TextCount + "1";
    gotoAndPlay(623);
} else {
    gotoAndPlay(644);
}
