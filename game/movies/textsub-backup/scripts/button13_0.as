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
