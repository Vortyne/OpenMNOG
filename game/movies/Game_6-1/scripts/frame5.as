#version 4
tellTarget ("/") {
    Movieisloaded = "1";
}
BGAction = "Canopy";
Score = "0";
trace("---------------------------------------");
trace("BEGIN GAME");
trace("---------------------------------------");
trace("Transitions:");
trace("Transition1 (Canopy -> Clouds): " add NestTransition1);
trace("Transition2 (Clouds -> Sky): " add NestTransition2);
trace("Transition3 (Sky->Clouds): " add NestTransition3);
trace("Transition4 (Clouds->Canopy): " add NestTransition4);
trace("Transition5 (Canopy->Jungle): " add NestTransition5);
trace("Transition6 (Jungle->Hive): " add NestTransition6);
trace("---------------------------------------");
trace("BG Section: " add BGAction);
trace("---------------------------------------");
