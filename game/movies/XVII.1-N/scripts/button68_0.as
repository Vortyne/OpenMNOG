#version 4
if (eval("/:LeKoro") eq "1") {
    tellTarget ("../Taipu") {
        gotoAndPlay("TurnTo");
    }
    tellTarget ("/") {
        MovieName = "XVII.1A.swf";
        BeginLoadMovie = "1";
        ShiftOff = "1";
    }
} else if (BubbleOpen eq "0") {
    BubbleOpen = "1";
    tellTarget ("../Taipu") {
        BubbleOpen = "1";
        gotoAndPlay("TurnTo");
    }
    gotoAndPlay(4);
} else {
    tellTarget ("../Taipu") {
        BubbleOpen = "0";
        gotoAndPlay("TurnBack");
    }
    BubbleOpen = "0";
    gotoAndPlay(2);
}
