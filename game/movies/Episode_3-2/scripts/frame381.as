#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/") {
        BookMovieName = "book.swf";
        BeginLoadMovie = "1";
    }
} else if (eval("/:MakuTells") eq "TaWahi") {
    tellTarget ("/") {
        MovieName = "I.6-S.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Sound_Waves.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "Sound_Beach.swf";
        Sound_ChangeBackgroundVolume = "1";
        Sound_BG_Volume = "High";
        ShiftOff = "1";
        Episode3_2Visited = "1";
        BeginLoadMovie = "1";
    }
} else {
    tellTarget ("/") {
        MovieName = "VII.1A.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "Amb_wind.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "Sound_Desert.swf";
        ShiftOff = "1";
        Episode3_2Visited = "1";
        BeginLoadMovie = "1";
    }
}
stop();
