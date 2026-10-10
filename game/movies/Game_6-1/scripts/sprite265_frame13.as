#version 4
RandHit = random("2");
if (RandHit eq "1") {
    set("../../:DamageLevel", eval("../../:DamageLevel") + "1");
}
