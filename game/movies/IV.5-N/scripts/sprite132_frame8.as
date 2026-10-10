#version 4
BubbleOpen = eval("../../ShrineSpeech/:BubbleOpen");
if (BubbleOpen eq "0") {
    tellTarget ("../TurnBack") {
        gotoAndPlay("play");
    }
    gotoAndStop(1);
} else {
    gotoAndPlay(7);
}
