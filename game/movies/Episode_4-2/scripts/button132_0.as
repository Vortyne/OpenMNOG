#version 4
if ("2" < SecondOne) {
    gotoAndPlay(649);
} else {
    Text = eval("Text" add SecondOne);
    SecondOne = SecondOne + "1";
    gotoAndPlay(613);
}
