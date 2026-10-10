#version 4
if (SimonStart eq "1") {
    if (SimonSays eq "none") {
    } else {
        tellTarget (SimonSays) {
            gotoAndPlay("play");
        }
        SimonStart = "0";
        SimonNotes = "";
    }
}
// Only the last notes matter (kept short).
if ("8" < SimonNotes.length) {
    SimonNotes = substring(SimonNotes, SimonNotes.length - "7", "8");
}
SongHeard = "";
if ("4" <= SimonNotes.length and substring(SimonNotes, SimonNotes.length - "3", "4") eq "ECBD") {
    SongHeard = "AllClear";
} else if (eval("/:KahuRide") eq "1" and "8" <= SimonNotes.length and substring(SimonNotes, SimonNotes.length - "7", "8") eq "BDACBDCE") {
    SongHeard = "KewaRide";
}
if (SongHeard ne "") {
    SongName = SongHeard;
    SimonNotes = "";
    tellTarget ("Correct") {
        gotoAndPlay("play");
    }
}
