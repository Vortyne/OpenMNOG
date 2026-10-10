#version 4
set("/:TamaruHealth", Health);
if (Health < "0") {
    tellTarget ("Rahi") {
        Change = "1";
        gotoAndPlay("KO");
    }
    gotoAndPlay(137);
}
