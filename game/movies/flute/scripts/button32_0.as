#version 4
SimonNotes = SimonNotes add "E";
if (NoteTarget eq "0") {
    loadMovie("FluteNoteE.swf", "NoteHolder");
    tellTarget ("Flute/keyE") {
        gotoAndPlay("Play");
    }
    NoteTarget = "1";
    NotePlaying1 = "0";
    Loaded = "0";
} else if (NoteTarget eq "1") {
    loadMovie("FluteNoteE.swf", "NoteHolder2");
    tellTarget ("Flute/keyE") {
        gotoAndPlay("Play");
    }
    NoteTarget = "2";
    NotePlaying2 = "0";
    Loaded = "0";
} else {
    loadMovie("FluteNoteE.swf", "NoteHolder3");
    tellTarget ("Flute/keyE") {
        gotoAndPlay("Play");
    }
    NoteTarget = "0";
    NotePlaying3 = "0";
    Loaded = "0";
}
