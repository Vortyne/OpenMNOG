#version 4
if (Attacking eq "0" and PrepAttack eq "0") {
    PrepAttack = "1";
    set("../:LastClick", name);
    Target = "";
}
