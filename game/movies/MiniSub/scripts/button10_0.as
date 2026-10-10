#version 4
tellTarget ("/") {
    MiniSubHit = "2";
    MovieName = eval("/holder/TextSub/:MovieName2");
    VerticalShift = eval("/holder/TextSub/:VerticalShift2");
    HorizontalShift = eval("/holder/TextSub/:HorizontalShift2");
    ShiftOff = eval("/holder/TextSub/:ShiftOff2");
    Sound_ChangeSFXMovie = eval("/holder/TextSub/:Sound_ChangeSFXMovie2");
    Sound_SFXMovie = eval("/holder/TextSub/:Sound_SFXMovie2");
    Music_ChangeMusic = eval("/holder/TextSub/:Music_ChangeMusic2");
    Music_Movie = eval("/holder/TextSub/:Music_Movie2");
    Sound_ChangeBackgroundMovie = eval("/holder/TextSub/:Sound_ChangeBackgroundMovie2");
    Sound_BackGround = eval("/holder/TextSub/:Sound_BackGround2");
    BeginLoadMovie = "1";
}
