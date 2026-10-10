#version 4
if (MapGuess eq "1") {
    tellTarget ("../") {
        TextToLoad = "Text_XX.5B_2_" add Language add ".txt";
    }
    loadMovie("TextSub.swf", "../Textsub");
} else {
    tellTarget ("../") {
        TextToLoad = "Text_XX.5B_3_" add Language add ".txt";
    }
    loadMovie("TextSub.swf", "../Textsub");
}
