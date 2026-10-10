#version 4
#operators flash5
if (eval("/:LostGame") != "1") {
    tellTarget ("SOS") {
        gotoAndPlay("Play");
    }
}
