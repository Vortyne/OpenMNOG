#version 4
if (BubbleOpen eq "0") {
    BubbleOpen = "1";
    set("../:BubbleChange", BubbleNum);
    tellTarget ("../") {
        set("BubbleOpen" add BubbleChange, "1");
    }
    gotoAndPlay(4);
} else {
    set("../:BubbleChange", BubbleNum);
    tellTarget ("../") {
        set("BubbleOpen" add BubbleChange, "0");
    }
    BubbleOpen = "0";
    gotoAndPlay(2);
}
