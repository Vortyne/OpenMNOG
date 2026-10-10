#version 4
if (eval("../:LastClick") eq "") {
} else {
    tellTarget ("AimTarget") {
        gotoAndPlay("Play");
    }
    set("../:passname", name);
    tellTarget ("../" add eval("../:LastClick")) {
        Target = eval("../:passname");
    }
    set("../:LastClick", "");
    LastClick = "";
}
