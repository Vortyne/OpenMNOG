#version 4
if (random("15") + "1" eq "1") {
    RandomElement = random("5") + "1";
    nextFrame();
    play();
} else {
    prevFrame();
    play();
}
