#version 4
CurrentTextSlot = eval("../:CurrentTextSlot");
GotoFrame = eval("TextSlot" add CurrentTextSlot);
if (GotoFrame ne "") {
    if (DoFadein eq "1") {
        DoFadein = "0";
        tellTarget ("Thumbnails") {
            gotoAndPlay(eval("../:GotoFrame"));
        }
    }
}
