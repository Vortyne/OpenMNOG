#version 4
if (eval("/:Pokoro") eq "1") {
    setProperty("/holder/Middle-Ground/X", _y, "2000");
}
tellTarget ("/") {
    Startedloading = "1";
}
