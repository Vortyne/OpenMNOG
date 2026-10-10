#version 4
BubbleOpen = eval("../../ShrineSpeech/:BubbleOpen");
if (BubbleOpen eq "1") {
    tellTarget ("../TurnToward") {
        gotoAndPlay("play");
    }
} else {
    gotoAndPlay(1);
}
