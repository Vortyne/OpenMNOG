#version 4
set("/:MakuHealth", Health);
if (Health < "0") {
    tellTarget ("Rahi") {
        Change = "1";
        gotoAndPlay("KO");
    }
    gotoAndPlay(157);
}
