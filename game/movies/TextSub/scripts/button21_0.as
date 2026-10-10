#version 4
setTarget("../");
if ("0" < Reply_2_Num) {
    SpeakerText = " ";
    LetterCount = "1";
    CloseQBox = "1";
    InputText = Reply_2_1;
    Segment = "1";
    Section = "2";
    CurrentSegments = Reply_2_Num;
    InputVoice = "Voice_" add Screen add "_" add Section add "_" add Segment add ".swf";
}
setTarget("");
