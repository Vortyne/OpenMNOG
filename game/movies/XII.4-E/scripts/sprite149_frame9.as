#version 4
RandNum = random("6") + "1";
if (RandNum eq "1") {
    tellTarget ("Keeper") {
        gotoAndPlay("Start");
    }
    gotoAndPlay(10);
} else {
    gotoAndPlay(1);
}
