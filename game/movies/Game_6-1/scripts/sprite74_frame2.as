#version 4
RandNum = random("8") + "1";
if (RandNum eq "1" and eval("../:StopBGElements") < "1") {
    set("../:ActiveBGElements", eval("../:ActiveBGElements") + "1");
    nextFrame();
    play();
} else {
    prevFrame();
    play();
}
