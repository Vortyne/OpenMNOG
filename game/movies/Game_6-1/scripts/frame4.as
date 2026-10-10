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
NestScale = "70";
NestJump = "1";
NestTransition1 = "120";
NestTransition2 = "135";
NestTransition3 = "190";
NestTransition4 = "210";
NestTransition5 = "230";
NestTransition6 = "250";
AllowFire = "1";
MagNum = "1";
EnemySpawnAllowed = "1";
TransitionRequest = "0";
EnemyActiveNum = "0";
DamageLevel = "0";
Shooting = "0";
Safetyloop = "0";
KillMsg = "1";
Win = "0";
Lose = "0";
