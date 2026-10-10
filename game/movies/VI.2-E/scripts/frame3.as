#version 4
Jewel = eval("/:Jewel");
if (Jewel eq "1") {
    setProperty("/holder/Middle-Ground/Jewel", _y, "-2000");
}
tellTarget ("/") {
    Startedloading = "1";
}
