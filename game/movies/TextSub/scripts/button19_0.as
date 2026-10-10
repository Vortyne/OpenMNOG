#version 4
setTarget("../");
if ("0" < Reply_3_Num) {
    SpeakerText = " ";
    LetterCount = "1";
    CloseQBox = "1";
    InputText = Reply_3_1;
    Segment = "1";
    Section = "3";
    CurrentSegments = Reply_3_Num;
    InputVoice = "Voice_" add Screen add "_" add Section add "_" add Segment add ".swf";
}
setTarget("");
