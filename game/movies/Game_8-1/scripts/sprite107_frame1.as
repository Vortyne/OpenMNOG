#version 4
BaseHealth = eval("../:BaseHealth");
Health = eval("../:Health");
HealthPercentage = Health / BaseHealth * "100";
HealthPercentage = int(HealthPercentage);
if (HealthPercentage < "1") {
    HealthPercentage = "1";
}
tellTarget ("HealthBar") {
    gotoAndStop(eval("../:HealthPercentage"));
}
