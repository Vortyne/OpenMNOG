#version 4
StopBGElements = "0";
ActiveBGElements = "0";
setProperty("GrowClouds", _xscale, "100" - eval("../../:NestTransition2") + eval("../../:NestScale"));
setProperty("GrowClouds", _yscale, "100" - eval("../../:NestTransition2") + eval("../../:NestScale"));
