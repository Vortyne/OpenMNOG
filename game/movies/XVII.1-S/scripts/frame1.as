#version 4
if (eval("/:Taipu") eq "XVII.1-S") {
    setProperty("Middle-Ground/Taipu", _y, "77");
    setProperty("Middle-Ground/TaipuBubble", _y, "77");
}
tellTarget ("/") {
    Startedloading = "1";
}
