#version 4
if (Reload eq "1") {
    tellTarget ("/holder/TextSub") {
        gotoAndPlay("BeginPrint");
    }
    Reload = "0";
}
GiveItem = "1";
gotoAndPlay(15);
