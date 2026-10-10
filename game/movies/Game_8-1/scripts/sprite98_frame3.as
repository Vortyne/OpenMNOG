#version 4
xPos = getProperty("../" add name, _x);
yPos = getProperty("../" add name, _y);
Health = eval("/:MakuHealth");
if (Health < "0") {
    Health = "0";
}
if (Health < BaseHealth) {
    AddHealth = int(BaseHealth * "0.25");
    Health = Health + AddHealth;
    if (BaseHealth < Health) {
        Health = BaseHealth;
    }
}
set("/:MakuHealth", Health);
