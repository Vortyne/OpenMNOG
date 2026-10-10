#version 4
PoKoro = eval("/:PoKoro");
Language = eval("/:Language");
if (PoKoro eq "1") {
    loadVariables("Text_VIII.4S_Fan_2_" add Language add ".txt", "FanBubble");
} else {
    loadVariables("Text_VIII.4S_Fan_1_" add Language add ".txt", "FanBubble");
}
stop();
