#version 4
BubbleOpen = "1";
tellTarget ("../GuardBubble") {
    BubbleOpen = "0";
    gotoAndPlay("PickText");
}
tellTarget ("../MinerBubble") {
    BubbleOpen = "0";
    gotoAndPlay("PickText");
}
Counter = Counter + "1";
if ("17" < Counter) {
    Counter = "1";
}
if (Counter eq "1" or Counter eq "3" or Counter eq "5" or Counter eq "7" or Counter eq "9" or Counter eq "11" or Counter eq "13" or Counter eq "14" or Counter eq "16") {
    Num = Num + "1";
    if ("9" < Num) {
        Num = "1";
    }
    Input = eval("Statement_" add Num);
    gotoAndPlay(4);
} else {
    Num2 = Num2 + "1";
    if ("8" < Num2) {
        Num2 = "1";
    }
    Input2 = eval("Statement2_" add Num2);
    gotoAndPlay(6);
}
