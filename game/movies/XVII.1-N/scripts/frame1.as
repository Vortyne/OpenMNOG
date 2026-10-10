#version 4
if (eval("/:Taipu") eq "XVII.1-N") {
    setProperty("Middle-Ground/Taipu", _y, "10");
    setProperty("Middle-Ground/TaipuBubble", _y, "10");
}
tellTarget ("/") {
    Startedloading = "1";
}
