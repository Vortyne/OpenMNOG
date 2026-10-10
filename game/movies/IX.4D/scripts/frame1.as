#version 4
loadMovie("sky1.swf", "/holder/Back-ground/Sky-holder");
Pokoro = eval("/:Pokoro");
QuarryKeyUsed = eval("/:QuarryKeyUsed");
if (Pokoro eq "1" and QuarryKeyUsed eq "3") {
    gotoAndPlay(3);
} else if (QuarryKeyUsed eq "3") {
    tellTarget ("Middle-Ground") {
        gotoAndPlay("Opened");
    }
}
