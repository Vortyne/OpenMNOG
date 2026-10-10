#version 4
Miss = "0";
if (Attacking eq "1") {
    tellTarget ("Rahi") {
        Change = "1";
        gotoAndPlay("Ready");
    }
    gotoAndPlay(22);
} else {
    tellTarget ("Rahi") {
        Change = "1";
        gotoAndPlay("idle");
    }
    gotoAndPlay(4);
}
