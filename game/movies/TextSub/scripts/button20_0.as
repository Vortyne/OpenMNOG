#version 4
setTarget("../");
if ("0" < Reply_4_Num) {
    SpeakerText = " ";
    LetterCount = "1";
    CloseQBox = "1";
    InputText = Reply_4_1;
    Segment = "1";
    Section = "4";
    CurrentSegments = Reply_4_Num;
    InputVoice = "Voice_" add Screen add "_" add Section add "_" add Segment add ".swf";
}
setTarget("");
