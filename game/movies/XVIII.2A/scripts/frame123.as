#version 4
if (eval("/:LiftLoc") eq "Up") {
    setTarget("/");
    MovieName = "XVIII.2A.swf";
    if (eval("/:Lekoro") eq "5") {
        Music_ChangeMusic = "1";
        Music_Movie = eval("blank:swf");
    }
    set("/:LiftLoc", "Down");
    Sound_ChangeSFXMovie = "1";
    Sound_SFXMovie = "Sound_Jungle.swf";
    Sound_ChangeBackgroundMovie = "1";
    Sound_Background = "Amb_Jungle.swf";
    ShiftOff = "1";
    BeginLoadMovie = "1";
    setTarget("");
} else if (eval("/:LiftLoc") eq "Down") {
    setTarget("/");
    MovieName = "XVIII.2A.swf";
    if (eval("/:Lekoro") eq "5") {
        Music_ChangeMusic = "1";
        Music_Movie = "Music_band.swf";
    }
    set("/:LiftLoc", "Up");
    Sound_ChangeSFXMovie = "1";
    Sound_SFXMovie = "blank.swf";
    Sound_ChangeBackgroundMovie = "1";
    Sound_Background = "Amb_Jungle.swf";
    ShiftOff = "1";
    BeginLoadMovie = "1";
    setTarget("");
}
stop();
