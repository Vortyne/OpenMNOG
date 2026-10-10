#version 4
if (eval("/Holder/:Talking") eq "1") {
    RandNum = random("3");
    if (RandNum eq "1") {
        gotoAndPlay(10);
        tellTarget ("../Matoro") {
            gotoAndPlay("Talk1");
        }
    } else if (RandNum eq "2") {
        gotoAndPlay(38);
        tellTarget ("../Matoro") {
            gotoAndPlay("Talk2");
        }
    } else {
        gotoAndPlay(66);
        tellTarget ("../Matoro") {
            gotoAndPlay("Talk3");
        }
    }
} else {
    gotoAndPlay(1);
}
