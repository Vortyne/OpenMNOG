#version 4
if (eval("/:LetterB") eq "1") {
    gotoAndPlay(1);
} else {
    section = eval("../textsub/:section");
    segment = eval("../textsub/:segment");
    if (section eq "3" and segment eq "5") {
        gotoAndPlay(3);
    } else {
        gotoAndPlay(1);
    }
}
