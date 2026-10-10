#version 4
if (eval("../../:NestTransition1") < eval("../../:NestScale") and eval("../../:NestScale") < eval("../../:NestTransition2")) {
    set("../../:BGAction", "Canopy_Clouds");
    set("../../:TransitionRequest", "1");
} else if (eval("../../:NestTransition5") < eval("../../:NestScale") and eval("../../:NestScale") < eval("../../:NestTransition6")) {
    set("../../:BGAction", "Canopy_Jungle");
    set("../../:TransitionRequest", "1");
}
set("../../:NestScale", eval("../../:NestScale") + eval("../../:NestJump"));
gotoAndPlay(3);
