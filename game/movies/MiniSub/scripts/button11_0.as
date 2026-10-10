#version 4
Question3 = eval("/holder/TextSub/:Question3");
if (Question3 eq "") {
} else {
    tellTarget ("/") {
        MiniSubHit = "3";
        MovieName = eval("/holder/TextSub/:MovieName3");
        VerticalShift = eval("/holder/TextSub/:VerticalShift3");
        HorizontalShift = eval("/holder/TextSub/:HorizontalShift3");
        ShiftOff = eval("/holder/TextSub/:ShiftOff3");
        Sound_ChangeSFXMovie = eval("/holder/TextSub/:Sound_ChangeSFXMovie3");
        Sound_SFXMovie = eval("/holder/TextSub/:Sound_SFXMovie3");
        Music_ChangeMusic = eval("/holder/TextSub/:Music_ChangeMusic3");
        Music_Movie = eval("/holder/TextSub/:Music_Movie3");
        Sound_ChangeBackgroundMovie = eval("/holder/TextSub/:Sound_ChangeBackgroundMovie3");
        Sound_BackGround = eval("/holder/TextSub/:Sound_BackGround3");
        BeginLoadMovie = "1";
    }
}
