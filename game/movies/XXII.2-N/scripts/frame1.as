#version 4
if (eval("/:IceTunnelLoc") eq "XXII.2-S") {
    gotoAndPlay(4);
} else if (eval("/:IceTunnelLoc") eq "XXII.3-N") {
    gotoAndPlay(7);
} else if (eval("/:IceTunnelLoc") eq "XXII.3-S") {
    gotoAndPlay(10);
} else if (eval("/:IceTunnelLoc") eq "XXII.4-N") {
    gotoAndPlay(13);
} else if (eval("/:IceTunnelLoc") eq "XXII.4-S") {
    gotoAndPlay(16);
}
tellTarget ("/") {
    Startedloading = "1";
}
