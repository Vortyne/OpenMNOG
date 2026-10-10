#version 4
RandVar = random("3");
if (Randvar eq "1") {
    gotoAndPlay(2);
} else if (RandVar eq "2") {
    gotoAndPlay(5);
} else {
    gotoAndPlay(8);
}
