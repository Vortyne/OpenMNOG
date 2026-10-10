#version 4
if (eval("/:LeKoro") < "2") {
    tellTarget ("/") {
        MovieName = "Episode_6-2.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "blank.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        BeginLoadMovie = "1";
    }
}
gotoAndPlay(21);
