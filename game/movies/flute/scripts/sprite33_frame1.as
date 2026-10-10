#version 4
NotePlaying1 = "1";
NotePlaying2 = "1";
NotePlaying3 = "1";
NoteTarget = "0";
SimonNotes = "nothing";
SongNotes = "empty";
if (eval("/:KahuRide") eq "0") {
    xKewa = getProperty("CartKewa", _x);
    setProperty("CartKewa", _x, "2000");
}
