#version 4
RandNum = random("20") + "1";
if (RandNum eq "1") {
    tellTarget ("Foot") {
        gotoAndStop(1);
    }
} else if (RandNum eq "2") {
    tellTarget ("Foot") {
        gotoAndStop(2);
    }
}
RandNum = random("30") + "1";
if (RandNum eq "1") {
    tellTarget ("Head") {
        gotoAndStop(1);
    }
} else if (RandNum eq "2") {
    tellTarget ("Head") {
        gotoAndStop(2);
    }
} else if (RandNum eq "3") {
    tellTarget ("Head") {
        gotoAndStop(3);
    }
}
RandNum = random("40") + "1";
if (RandNum eq "1") {
    tellTarget ("ArmL") {
        gotoAndStop(1);
    }
    tellTarget ("ArmR") {
        gotoAndStop(1);
    }
} else if (RandNum eq "2") {
    tellTarget ("ArmL") {
        gotoAndStop(2);
    }
    tellTarget ("ArmR") {
        gotoAndStop(2);
    }
}
gotoAndPlay(1);
