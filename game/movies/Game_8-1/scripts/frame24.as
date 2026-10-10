#version 4
if ("2" < FightVisions) {
    tellTarget ("/") {
        MovieName = "Episode_9-1.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "blank.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        BeginLoadMovie = "1";
    }
} else {
    if ("3" < FightScreen) {
    } else {
        set("../:FightScreen", eval("../:FightScreen") + "1");
    }
    set("/:MovieName", "XXIV." add eval("/:FightScreen") add ".swf");
    tellTarget ("/") {
        VerticalShift = "0";
        HorizontalShift = "1";
        ShiftOff = "0";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "blank.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        BeginLoadMovie = "1";
    }
}
