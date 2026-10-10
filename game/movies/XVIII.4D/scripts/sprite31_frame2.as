#version 4
if (eval("/:KahuRide") eq "1") {
    gotoAndPlay(1);
} else {
    section = eval("../textsub/:section");
    segment = eval("../textsub/:segment");
    if (segment eq "2") {
        gotoAndPlay(3);
    } else {
        gotoAndPlay(1);
    }
}
