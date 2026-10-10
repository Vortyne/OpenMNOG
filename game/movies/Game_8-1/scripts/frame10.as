#version 4
if (FightVisions < "3") {
    set("../:FightWins", "0");
    set("../:FightVisions", eval("../:FightVisions") + "1");
    if ("1" < eval("/:FightScreen")) {
        set("/:FightScreen", eval("/:FightScreen") - "1");
    }
    if (eval("../:FightVisions") eq "3") {
        tellTarget ("/") {
            MovieName = "Episode_8-8.swf";
        }
    } else if (eval("../:FightVisions") eq "2") {
        tellTarget ("/") {
            MovieName = "Episode_8-7.swf";
        }
    } else {
        tellTarget ("/") {
            MovieName = "Episode_8-6.swf";
        }
    }
    tellTarget ("/") {
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "blank.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        BeginLoadMovie = "1";
    }
} else if ("2" < FightVisions) {
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
    set("../:FightWins", eval("../:FightWins") + "1");
    if ("1" < FightScreen) {
        set("../:FightScreen", eval("../:FightScreen") - "1");
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
