#version 5
RandNum = random(12) + 1;
if (RandNum eq 1) {
    gotoAndPlay(14);
} else if (RandNum eq 2) {
    gotoAndPlay(72);
} else if (RandNum eq 3) {
    gotoAndPlay(124);
} else if (RandNum eq 4) {
    gotoAndPlay(177);
} else {
    gotoAndPlay(1);
}
