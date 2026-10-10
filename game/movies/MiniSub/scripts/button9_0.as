#version 4
tellTarget ("/") {
    MiniSubHit = "1";
    MovieName = eval("/holder/TextSub/:MovieName1");
    VerticalShift = eval("/holder/TextSub/:VerticalShift1");
    HorizontalShift = eval("/holder/TextSub/:HorizontalShift1");
    ShiftOff = eval("/holder/TextSub/:ShiftOff1");
    Sound_ChangeSFXMovie = eval("/holder/TextSub/:Sound_ChangeSFXMovie1");
    Sound_SFXMovie = eval("/holder/TextSub/:Sound_SFXMovie1");
    Music_ChangeMusic = eval("/holder/TextSub/:Music_ChangeMusic1");
    Music_Movie = eval("/holder/TextSub/:Music_Movie1");
    Sound_ChangeBackgroundMovie = eval("/holder/TextSub/:Sound_ChangeBackgroundMovie1");
    Sound_BackGround = eval("/holder/TextSub/:Sound_BackGround1");
    BeginLoadMovie = "1";
}
