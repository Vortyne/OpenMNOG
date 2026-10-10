#version 4
RandNum = random("2") + "1";
if (RandNum eq "2") {
    tellTarget ("Score") {
        gotoAndStop(2);
    }
}
