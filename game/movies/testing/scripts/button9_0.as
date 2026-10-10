#version 4
if ("0" < Link_Exists and Link_Section eq Section and Link_Segment eq Segment) {
    tellTarget ("/") {
        MovieName = eval("/holder/Subtext/:MovieName");
        VerticalShift = eval("/holder/Subtext/:VerticalShift");
        HorizontalShift = eval("/holder/Subtext/:HorizontalShift");
        BeginLoadMovie = "1";
    }
} else if (Segment < CurrentSegments) {
    if (OpenIsDone eq "1") {
        tellTarget ("../../") {
            SpeakerText = " ";
            LetterCount = "1";
            Segment = Segment + "1";
            InputText = eval("Reply_" add Section add "_" add Segment);
        }
        gotoAndStop(1);
    } else {
        tellTarget ("../../") {
            SpeakerText = " ";
            LetterCount = "1";
            Segment = Segment + "1";
            InputText = eval("OpenStatement" add Segment);
        }
        gotoAndStop(1);
    }
} else {
    tellTarget ("../../") {
        SpeakerText = AskMe;
        InputText = AskMe;
        LetterCount = eval("AskMe:length");
        OpenQBox = "1";
    }
}
