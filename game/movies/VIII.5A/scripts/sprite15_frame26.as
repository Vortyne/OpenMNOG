#version 4
RandCough = random("8");
if (RandCough eq "1") {
    gotoAndPlay(27);
} else {
    gotoAndPlay(1);
}
