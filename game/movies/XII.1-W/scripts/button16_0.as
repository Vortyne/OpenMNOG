#version 4
if ("5" < eval("/:Quest")) {
    tellTarget ("/") {
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "blank.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "Music_OnuKoro_2.swf";
    }
} else {
    tellTarget ("/") {
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Amb_tunnel.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "Music_OnuKoro.swf";
    }
}
tellTarget ("/") {
    MovieName = "XI.7-W.swf";
    BeginLoadMovie = "1";
}
