#version 4
RotateIsOn = "0";
ShiftIsOn = "0";
Time_Start = "0";
loadMovie("SpecialSFX_Telescope.swf", "/holder/SFX-holder", "GET");
DistanceBetween = "300";
Launch0 = eval("/:Launch0");
Launch1 = eval("/:Launch1");
Launch2 = eval("/:Launch2");
Launch3 = eval("/:Launch3");
Launch4 = eval("/:Launch4");
Launch5 = eval("/:Launch5");
Launch6 = eval("/:Launch6");
Launch7 = eval("/:Launch7");
Launch8 = eval("/:Launch8");
Launch9 = "1009925424";
ServerBasedDate = "978325200";
if (eval("/:I4WVisited") eq "1") {
    ServerBasedDate = "981054000";
    if (eval("/:TaVisit") eq "1") {
        ServerBasedDate = "982868400";
    }
    if (eval("/:GaKoroVisited") eq "1") {
        ServerBasedDate = "984682800";
    }
    if (eval("/:GaKoro") eq "1") {
        ServerBasedDate = "984942000";
        if (eval("/:KohliBallUsed") eq "1") {
            ServerBasedDate = "989348400";
        }
        if (eval("/:PoKoro") eq "1" or "0" < eval("/:QuarryKeyUsed")) {
            ServerBasedDate = "989953200";
            if (eval("/:Chisel") eq "1") {
                ServerBasedDate = "992631600";
                if (eval("/:OnukoroLava") eq "1") {
                    ServerBasedDate = "995223600";
                    if (eval("/:LeKoro") >= "1") {
                        ServerBasedDate = "997642800";
                        if (eval("/:LeKoro") >= "2") {
                            ServerBasedDate = "999370800";
                            if (eval("/:Ensign") eq "1") {
                                ServerBasedDate = "999716400";
                                if (eval("/:KoKoro") >= "5") {
                                    ServerBasedDate = "1000148401";
                                    if (eval("/:LetterB") eq "1") {
                                        ServerBasedDate = "1001530800";
                                        if (eval("/:cKapura") eq "1") {
                                            ServerBasedDate = "1002308400";
                                            if (eval("/:Quest") >= "1") {
                                                ServerBasedDate = "1003172400";
                                                if (eval("/:Quest") >= "5") {
                                                    ServerBasedDate = "1004036401";
                                                    if (eval("/:Quest") >= "6") {
                                                        ServerBasedDate = "1009220400";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
TimeLoopCounter = "1";
while (eval("Launch" add TimeLoopCounter) < ServerBasedDate) {
    TimeLoopCounter = TimeLoopCounter + "1";
}
LaunchNext = eval("Launch" add TimeLoopCounter);
LaunchLast = eval("Launch" add (TimeLoopCounter - "1"));
Time_Start = "0";
if (Launch0 < ServerBasedDate) {
    setProperty("/holder/Sky/Constellations", _x, getProperty("/holder/Sky/Constellations", _x) - (TimeLoopCounter - "1") * DistanceBetween - (ServerBasedDate - LaunchLast) / (LaunchNext - LaunchLast) * DistanceBetween);
}
