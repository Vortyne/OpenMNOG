#version 4
setTarget("/");
if (eval("/:LeKoro") eq "2") {
    MovieName = "Episode_6-3.swf";
    Sound_ChangeBackgroundMovie = "1";
    Sound_Background = "blank.swf";
    Sound_ChangeSFXMovie = "1";
    Sound_SFXMovie = "blank.swf";
    Music_ChangeMusic = "1";
    Music_Movie = "blank.swf";
} else {
    MovieName = "XVIII.5-S.swf";
}
BeginLoadMovie = "1";
setTarget("");
