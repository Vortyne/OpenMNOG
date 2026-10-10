#version 4
if (TextSubLoaded eq "1") {
    if (Count < "130") {
        SlideStep = "5" * eval("_level0:SlideFactor");
        if (SlideStep < "1") {
            SlideStep = "5";
        }
        if ("130" - Count < SlideStep) {
            SlideStep = "130" - Count;
        }
        setProperty("/holder/TextSub", _y, getProperty("/holder/TextSub", _y) - SlideStep);
        Count = Count + SlideStep;
        gotoAndPlay(15);
    }
} else {
    gotoAndPlay(15);
}
