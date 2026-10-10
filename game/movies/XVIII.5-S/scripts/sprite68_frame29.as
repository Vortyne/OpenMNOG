#version 4
RandNum = random("4") + "1";
if (RandNum eq "1") {
    tellTarget ("Wing1") {
        play();
    }
    tellTarget ("Wing2") {
        play();
    }
}
