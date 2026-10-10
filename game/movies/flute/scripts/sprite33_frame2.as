#version 4
if (NotePlaying1 eq "0") {
    if (Loaded eq "1") {
        tellTarget ("NoteHolder") {
            gotoAndPlay("play");
        }
        NotePlaying1 = "1";
    }
} else if (NotePlaying2 eq "0") {
    if (Loaded eq "1") {
        tellTarget ("NoteHolder2") {
            gotoAndPlay("play");
        }
        NotePlaying2 = "1";
    }
} else if (NotePlaying3 eq "0") {
    if (Loaded eq "1") {
        tellTarget ("NoteHolder3") {
            gotoAndPlay("play");
        }
        NotePlaying3 = "1";
    }
}
if (eval("/:KahuRide") eq "1") {
    setProperty("CartKewa", _x, xKewa);
}
