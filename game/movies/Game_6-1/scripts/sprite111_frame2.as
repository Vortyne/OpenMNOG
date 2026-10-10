#version 4
RandNum = random("50") + "1";
if (RandNum eq "1") {
    nextFrame();
    play();
} else {
    prevFrame();
    play();
}
