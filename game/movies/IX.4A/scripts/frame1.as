#version 4
loadMovie("sky1.swf", "/holder/Back-ground/Sky-holder");
Pokoro = eval("/:Pokoro");
QuarryKeyUsed = eval("/:QuarryKeyUsed");
if (Pokoro eq "1" and QuarryKeyUsed eq "4") {
    gotoAndPlay(3);
} else if (QuarryKeyUsed eq "4") {
    tellTarget ("Middle-Ground") {
        gotoAndPlay("Opened");
    }
}
