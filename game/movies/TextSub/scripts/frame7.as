#version 4
if (Counter eq "0") {
    HeightStep = eval("_level0:QBoxStep");
    if (HeightStep eq "") {
        HeightStep = "4";
    }
}
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
} else if (eval("/:AllowTextSub") eq "1") {
    if (length(InputText) + "2" >= LetterCount) {
        TextStep = eval("_level0:TextStep");
        if (TextStep eq "") {
            TextStep = "2";
        }
        LetterCount = LetterCount + TextStep - "2";
        SpeakerText = substring(InputText, "0", LetterCount);
        LetterCount = LetterCount + "2";
        if (InputText eq AskMe) {
            LetterCount = LetterCount + "1";
        }
        tellTarget ("/holder") {
            Talking = "1";
        }
    } else {
        tellTarget ("/holder") {
            Talking = "0";
        }
        if (QBoxUp eq "0") {
            tellTarget ("QBox/NextMovie") {
                gotoAndPlay("AllowNext");
            }
        }
    }
}
