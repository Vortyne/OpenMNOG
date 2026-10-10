#version 4
if (Health < "0") {
    tellTarget ("Rahi") {
        Change = "1";
        gotoAndPlay("KO");
    }
    gotoAndPlay(102);
}
