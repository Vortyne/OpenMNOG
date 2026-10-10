#version 4
if (Exit_Section eq Section and Exit_Segment eq Segment) {
    tellTarget ("/") {
        MovieName = eval("/holder/TextSub/:MovieName");
        VerticalShift = eval("/holder/TextSub/:VerticalShift");
        HorizontalShift = eval("/holder/TextSub/:HorizontalShift");
        ShiftOff = eval("/holder/TextSub/:ShiftOff");
        Sound_ChangeSFXMovie = eval("/holder/TextSub/:Sound_ChangeSFXMovie");
        Sound_SFXMovie = eval("/holder/TextSub/:Sound_SFXMovie");
        Music_ChangeMusic = eval("/holder/TextSub/:Music_ChangeMusic");
        Music_Movie = eval("/holder/TextSub/:Music_Movie");
        Sound_ChangeBackgroundMovie = eval("/holder/TextSub/:Sound_ChangeBackgroundMovie");
        Sound_BackGround = eval("/holder/TextSub/:Sound_BackGround");
        BeginLoadMovie = "1";
    }
} else if (Exit2_Section eq Section and Exit2_Segment eq Segment) {
    tellTarget ("/") {
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
} else if (Exit3_Section eq Section and Exit3_Segment eq Segment) {
    tellTarget ("/") {
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
} else if (Exit4_Section eq Section and Exit4_Segment eq Segment) {
    tellTarget ("/") {
        MovieName = eval("/holder/TextSub/:MovieName4");
        VerticalShift = eval("/holder/TextSub/:VerticalShift4");
        HorizontalShift = eval("/holder/TextSub/:HorizontalShift4");
        ShiftOff = eval("/holder/TextSub/:ShiftOff4");
        Sound_ChangeSFXMovie = eval("/holder/TextSub/:Sound_ChangeSFXMovie4");
        Sound_SFXMovie = eval("/holder/TextSub/:Sound_SFXMovie4");
        Music_ChangeMusic = eval("/holder/TextSub/:Music_ChangeMusic4");
        Music_Movie = eval("/holder/TextSub/:Music_Movie4");
        Sound_ChangeBackgroundMovie = eval("/holder/TextSub/:Sound_ChangeBackgroundMovie4");
        Sound_BackGround = eval("/holder/TextSub/:Sound_BackGround4");
        BeginLoadMovie = "1";
    }
} else if (TextLoad_Section eq Section and TextLoad_Segment eq Segment) {
    tellTarget ("../../") {
        TextIsLoaded = "0";
        Language = eval("/:Language");
        TextToLoad = TextLoad_Filename add Language add ".txt";
        loadVariables(TextToLoad, "/holder/TextSub");
        SpeakerText = " ";
        LetterCount = "1";
        OpenQBox = "1";
        gotoAndPlay("BeginPrint");
    }
} else if (Segment < CurrentSegments) {
    if (OpenIsDone eq "1") {
        tellTarget ("../../") {
            SpeakerText = " ";
            LetterCount = "1";
            Segment = Segment + "1";
            InputText = eval("Reply_" add Section add "_" add Segment);
            InputVoice = "Voice_" add Screen add "_" add Section add "_" add Segment add ".swf";
        }
        gotoAndStop(1);
    } else {
        tellTarget ("../../") {
            SpeakerText = " ";
            LetterCount = "1";
            Segment = Segment + "1";
            InputText = eval("OpenStatement" add Segment);
            InputVoice = "Voice_" add OpenScreen add "_OpenStatement_" add Segment add ".swf";
        }
        gotoAndStop(1);
    }
} else {
    tellTarget ("../../") {
        SpeakerText = " ";
        InputText = AskMe;
        LetterCount = "1";
        InputVoice = "Voice_" add Screen add "_AskMe.swf";
        OpenQBox = "1";
    }
}
