#version 4
if (eval("/:Taipu") eq "XVII.2-S") {
    setProperty("Middle-Ground/Taipu", _y, "-17.1");
    setProperty("Middle-Ground/TaipuBubble", _y, "-17.1");
}
tellTarget ("/") {
    Startedloading = "1";
}
