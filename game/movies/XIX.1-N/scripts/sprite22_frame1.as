#version 4
RandNum = random("4");
if (RandNum eq "1") {
    tellTarget ("CreakSound") {
        gotoAndPlay("Play");
    }
}
