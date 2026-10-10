#version 4
tellTarget ("/") {
    Startedloading = "1";
}
if (eval("/:cKopeke") eq "1") {
    setProperty("Middle-Ground/Kopeke", _x, "2000");
}
