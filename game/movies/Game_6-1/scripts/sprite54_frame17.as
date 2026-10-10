#version 4
if (eval("../../:NestTransition3") < eval("../../:NestScale")) {
    if (ActiveBGElements eq "0") {
        set("../../:BGAction", "Sky_Clouds");
        set("../../:TransitionRequest", "1");
    } else {
        trace("Asking BG elements to stop spawning. Active BG Elements: " add ActiveBGElements);
        StopBGElements = "1";
    }
}
set("../../:NestScale", eval("../../:NestScale") + eval("../../:NestJump"));
setProperty("GrowClouds", _xscale, "100" - eval("../../:NestTransition2") + eval("../../:NestScale"));
setProperty("GrowClouds", _yscale, "100" - eval("../../:NestTransition2") + eval("../../:NestScale"));
gotoAndPlay(3);
