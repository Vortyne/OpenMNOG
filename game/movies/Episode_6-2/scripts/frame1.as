#version 4
tellTarget ("/") {
    Startedloading = "1";
}
RandNum = random("100") + "1";
if (RandNum eq "1") {
    set("/:Cheat", "3");
} else {
    set("/:Cheat", "0");
}
