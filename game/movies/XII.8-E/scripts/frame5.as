#version 4
if (eval("/:OnuKoroDoor") eq "1") {
    tellTarget ("/holder/Middle-Ground/Door") {
        gotoAndStop(3);
    }
} else {
    tellTarget ("/holder/Middle-Ground/Door") {
        gotoAndStop(2);
    }
}
