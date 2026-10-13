#version 4
// OpenMNOG: sailing from Ga-Koro (to Po-Wahi or Ta-Wahi: only Ga-Koro's boat goes there), Ga-Koro's own music (the
// background sound, SFX_WaterAmbiant) stops: the sail plays its shorter Ga-Koro music instead. (The original held the
// background's music at 12%, barely heard; its high-quality replacement plays at full volume, over the sail's.) Each
// way the sail ends loads a new background.
if (eval("/:SailTo") ne "GaKoro") {
    loadMovie("blank.swf", "/Sound-Background");
}
