#version 4
if (length(InputText) + "2" >= LetterCount) {
    SpeakerText = substring(InputText, "0", LetterCount);
    LetterCount = LetterCount + "2";
}
