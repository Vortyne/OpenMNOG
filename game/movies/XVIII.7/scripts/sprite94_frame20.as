#version 4
RandNum = random("20") + "1";
if (RandNum eq "1") {
    if (eval("/:TipToe") eq "1") {
        tellTarget ("Taipu") {
            gotoAndPlay("Jump");
        }
    }
    nextFrame();
    play();
} else {
    gotoAndPlay(1);
}
