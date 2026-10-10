#version 4
if (QBoxUp eq "0" and OpenQBox eq "1") {
    OpenIsDone = "1";
    tellTarget ("QBox/NextMovie") {
        gotoAndStop("Clear");
    }
    if (HeightDiff >= Counter) {
        Counter = Counter + HeightStep;
        setProperty("QBox", _y, getProperty("QBox", _y) - HeightStep);
        setProperty("BackBox", _y, getProperty("BackBox", _y) - HeightStep);
    } else {
        QBoxUp = "1";
        Counter = "0";
        OpenQBox = "0";
    }
} else if (QBoxUp eq "1" and CloseQBox eq "1") {
    tellTarget ("QBox/NextMovie") {
        gotoAndStop("Clear");
    }
    if (HeightDiff >= Counter) {
        Counter = Counter + HeightStep;
        setProperty("QBox", _y, getProperty("QBox", _y) + HeightStep);
        setProperty("BackBox", _y, getProperty("BackBox", _y) + HeightStep);
    } else {
        LetterCount = "1";
        QBoxUp = "0";
        Counter = "0";
        CloseQBox = "0";
    }
} else if (eval("InputText:length") >= LetterCount) {
    SpeakerText = substring(InputText, "0", LetterCount);
    LetterCount = LetterCount + "1";
} else if (QBoxUp eq "0") {
    tellTarget ("QBox/NextMovie") {
        gotoAndPlay("AllowNext");
    }
}
