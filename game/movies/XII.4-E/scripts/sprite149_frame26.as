#version 4
RandNum = random("20") + "1";
if (RandNum eq "1") {
    tellTarget ("Keeper") {
        gotoAndPlay("Play2");
    }
    gotoAndPlay(27);
} else {
    gotoAndPlay(23);
}
