#version 4
RandNum = random("3") + "1";
if (RandNum eq "1") {
    nextFrame();
    play();
} else {
    prevFrame();
    play();
}
