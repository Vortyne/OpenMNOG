#version 4
RandNum = random("10") + "1" + eval("/:Skill");
if (RandNum eq "1") {
    nextFrame();
    play();
} else {
    prevFrame();
    play();
}
