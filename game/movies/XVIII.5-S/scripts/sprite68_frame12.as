#version 4
RandNum = random("18") + "1";
if (RandNum eq "1") {
    gotoAndPlay(13);
} else if (RandNum eq "2") {
    gotoAndPlay(29);
} else if (RandNum eq "3") {
    gotoAndPlay(48);
} else if (RandNum eq "3") {
    tellTarget ("Wing1") {
        play();
    }
    tellTarget ("Wing2") {
        play();
    }
} else {
    gotoAndPlay(1);
}
