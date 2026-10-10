#version 4
if (Health < "1") {
    tellTarget ("Rahi") {
        Change = "1";
        gotoAndPlay("KO");
    }
    gotoAndPlay(146);
}
