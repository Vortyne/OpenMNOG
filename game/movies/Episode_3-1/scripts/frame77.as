#version 4
FromGakoro = eval("/:FromGakoro");
SailTo = eval("/:SailTo");
if (SailTo eq "GaKoro") {
    gotoAndPlay(179);
} else if (SailTo eq "Ta-Wahi") {
    gotoAndPlay(280);
} else if (SailTo eq "Po-Wahi") {
    gotoAndPlay(78);
}
