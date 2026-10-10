#version 4
if (eval("../../:NestTransition6") < eval("../../:NestScale")) {
    if (ActiveBGElements eq "0") {
        set("../../:BGAction", "Jungle_Hive");
        set("../../:TransitionRequest", "1");
    } else {
        trace("Asking BG elements to stop spawning. Active BG Elements: " add ActiveBGElements);
        StopBGElements = "1";
    }
}
set("../../:NestScale", eval("../../:NestScale") + eval("../../:NestJump"));
setProperty("Hive", _xscale, "100" + eval("../../:NestScale") - eval("../../:NestTransition5"));
setProperty("Hive", _yscale, "100" + eval("../../:NestScale") - eval("../../:NestTransition5"));
gotoAndPlay(3);
