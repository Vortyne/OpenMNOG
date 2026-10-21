#version 4
Language = eval("/:Language");
TextToLoad = "Text_Game_6-1_" add Language add ".txt";
loadVariables(TextToLoad, "PopUp");
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:LeKoro") < "1") {
    startDrag("MouseDrag", "1");
}
// OpenMNOG: Kongu's message and Tamaru's bird now play out before the Nui-Rama come (sprite146_frame73.as), 57
// frames later than before (36 after a loss, when the bird doesn't fly and they come when the message is over); the
// nest starts that much further off (3.8 or 2.4 of its steps of 1 every 15 frames), so it's where it was when they
// come. (Was "70".)
if (eval("/:LostGame") eq "1") {
    NestScale = "67.6";
} else {
    NestScale = "66.2";
}
NestJump = "1";
NestTransition1 = "120";
NestTransition2 = "135";
NestTransition3 = "190";
NestTransition4 = "210";
NestTransition5 = "230";
NestTransition6 = "250";
AllowFire = "1";
MagNum = "1";
// (OpenMNOG: none until Kongu's message is over. Was "1".)
EnemySpawnAllowed = "0";
TransitionRequest = "0";
EnemyActiveNum = "0";
DamageLevel = "0";
Shooting = "0";
Safetyloop = "0";
KillMsg = "1";
Win = "0";
Lose = "0";
