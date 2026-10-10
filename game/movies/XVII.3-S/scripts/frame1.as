#version 4
if (eval("/:Taipu") eq "XVII.3-S") {
    setProperty("Middle-Ground/Taipu", _y, "-123");
} else {
    setProperty("Middle-Ground/Button_Forward", _y, "0");
    setProperty("Middle-Ground/Taipu", _y, "-2000");
}
tellTarget ("/") {
    Startedloading = "1";
}
