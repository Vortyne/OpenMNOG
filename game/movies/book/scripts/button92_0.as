#version 4
BookMovieName = eval("MovieSlot" add CurrentTextSlot);
tellTarget ("/") {
    // (The slot played, for the chronicle to open at next time.)
    ChronicleSlot = eval("/holder/:CurrentTextSlot");
    BookMovieName = eval("/holder/:BookMovieName");
    BeginLoadMovie = "1";
}
