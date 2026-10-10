#version 4
if (BubbleOpen eq "0") {
    BubbleOpen = "1";
    tellTarget ("../") {
        BubbleOpen = "1";
    }
    gotoAndPlay(4);
} else {
    tellTarget ("../") {
        BubbleOpen = "0";
    }
    BubbleOpen = "0";
    gotoAndPlay(2);
}
