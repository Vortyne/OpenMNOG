#version 4
if (eval("../../:NestTransition4") < eval("../../:NestScale")) {
    set("../../:BGAction", "Clouds_Canopy");
    set("../../:TransitionRequest", "1");
} else if (eval("../../:NestTransition2") < eval("../../:NestScale") and eval("../../:NestScale") < eval("../../:NestTransition3")) {
    set("../../:BGAction", "Clouds_Sky");
    set("../../:TransitionRequest", "1");
}
set("../../:NestScale", eval("../../:NestScale") + eval("../../:NestJump"));
gotoAndPlay(3);
