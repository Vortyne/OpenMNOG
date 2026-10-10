#version 4
Destination = eval("/:Destination");
tellTarget ("/") {
    MovieName = Destination;
    Sound_ChangeBackgroundMovie = "1";
    Sound_Background = "Amb_wind.swf";
    Sound_ChangeSFXMovie = "1";
    Sound_SFXMovie = "blank.swf";
    Music_ChangeMusic = "1";
    Music_Movie = "blank.swf";
    ShiftOff = "0";
    BeginLoadMovie = "1";
}
stop();
