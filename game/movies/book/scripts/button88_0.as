#version 4
// (OpenMNOG: opened from the ending prompt, which can't otherwise be left: closing it brings the prompt back. Checked
// first: the Quest is still under 7 at the end of the last episode, which sent it back into that episode.)
if (eval("/:EndingChronicle") eq "1") {
    // (The Chronicle gone first: the prompt over a black screen.)
    loadMovie("blank.swf", "/holder");
    getURL("FSCommand:gameend", "");
} else if (eval("/:Quest") < "7") {
    tellTarget ("/") {
        BookOpen = "0";
        BeginLoadMovie = "1";
    }
}
