#version 4
if (eval("/:Quest") < "7") {
    tellTarget ("/") {
        BookOpen = "0";
        BeginLoadMovie = "1";
    }
} else if (eval("/:EndingChronicle") eq "1") {
    // (Opened from the ending prompt, which can't otherwise be left: closing it brings the prompt back.)
    getURL("FSCommand:gameend", "");
}
