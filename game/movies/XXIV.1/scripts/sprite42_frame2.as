#version 4
CurrentTime = getTimer();
if ("30000" < CurrentTime - StartTime) {
    tellTarget ("/") {
        MovieName = "Episode_8-9.swf";
        ShiftOff = "1";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "blank.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        BeginLoadMovie = "1";
    }
    gotoAndPlay(5);
}
