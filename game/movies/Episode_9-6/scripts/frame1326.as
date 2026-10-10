#version 5
BookOpen = eval("/:BookOpen");
if (BookOpen eq 1) {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = 1;
    }
} else {
    getURL("FSCommand:gameend", "");
}
stop();
