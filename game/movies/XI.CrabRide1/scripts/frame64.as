#version 4
if (eval("/:Destination") eq "XI.1-S.swf") {
    tellTarget ("/") {
        Music_ChangeMusic = "1";
        Music_Movie = "Music_OnuKoro.swf";
    }
}
tellTarget ("/") {
    MovieName = eval("/:Destination");
    ShiftOff = "0";
    BeginLoadMovie = "1";
}
stop();
