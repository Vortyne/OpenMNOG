#version 4
set("/:KapuraHealth", Health);
if (Health < "0") {
    tellTarget ("Rahi") {
        Change = "1";
        gotoAndPlay("KO");
    }
    gotoAndPlay(153);
}
