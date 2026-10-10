#version 4
BubbleOpen = "1";
tellTarget ("../GuildBubble") {
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
if (Counter eq "1" or Counter eq "2" or Counter eq "4" or Counter eq "6" or Counter eq "8" or Counter eq "10" or Counter eq "12" or Counter eq "14" or Counter eq "16") {
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
