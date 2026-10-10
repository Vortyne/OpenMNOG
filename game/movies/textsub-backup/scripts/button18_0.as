#version 4
setTarget("../");
if ("0" < Reply_1_Num) {
    SpeakerText = " ";
    LetterCount = "1";
    CloseQBox = "1";
    InputText = Reply_1_1;
    Segment = "1";
    Section = "1";
    CurrentSegments = Reply_1_Num;
    InputVoice = "Voice_" add Screen add "_" add Section add "_" add Segment add ".swf";
}
setTarget("");
