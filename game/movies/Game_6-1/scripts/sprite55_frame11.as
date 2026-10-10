#version 4
set("../../:EnemySpawnAllowed", "1");
set("../../:BGAction", "Clouds");
trace("Transition Successful");
trace("---------------------------------------");
trace("BG Section: " add eval("../../:BGAction"));
trace("---------------------------------------");
tellTarget ("../") {
    gotoAndStop(eval("../:BGAction"));
}
