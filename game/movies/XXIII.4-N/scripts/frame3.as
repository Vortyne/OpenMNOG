#version 4
tellTarget ("/") {
    Movieisloaded = "1";
}
set("/:Loc", "1-N");
stop();
