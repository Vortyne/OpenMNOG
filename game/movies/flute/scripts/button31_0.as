#version 4
SimonNotes = SimonNotes add "D";
if (NoteTarget eq "0") {
    loadMovie("FluteNoteD.swf", "NoteHolder");
    tellTarget ("Flute/keyD") {
        gotoAndPlay("Play");
    }
    NoteTarget = "1";
    NotePlaying1 = "0";
    Loaded = "0";
} else if (NoteTarget eq "1") {
    loadMovie("FluteNoteD.swf", "NoteHolder2");
    tellTarget ("Flute/keyD") {
        gotoAndPlay("Play");
    }
    NoteTarget = "2";
    NotePlaying2 = "0";
    Loaded = "0";
} else {
    loadMovie("FluteNoteD.swf", "NoteHolder3");
    tellTarget ("Flute/keyD") {
        gotoAndPlay("Play");
    }
    NoteTarget = "0";
    NotePlaying3 = "0";
    Loaded = "0";
}
