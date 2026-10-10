#version 4
if (TextSubLoaded eq "1") {
    if (Count < "108") {
        SlideStep = "4" * eval("_level0:SlideFactor");
        if (SlideStep < "1") {
            SlideStep = "4";
        }
        if ("108" - Count < SlideStep) {
            SlideStep = "108" - Count;
        }
        setProperty("/holder/TextSub", _y, getProperty("/holder/TextSub", _y) - SlideStep);
        Count = Count + SlideStep;
        gotoAndPlay(21);
    }
} else {
    gotoAndPlay(21);
}
