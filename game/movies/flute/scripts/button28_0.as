#version 4
SimonNotes = SimonNotes add "A";
if (NoteTarget eq "0") {
    loadMovie("FluteNoteA.swf", "NoteHolder");
    tellTarget ("Flute/keyA") {
        gotoAndPlay("Play");
    }
    NoteTarget = "1";
    NotePlaying1 = "0";
    Loaded = "0";
} else if (NoteTarget eq "1") {
    loadMovie("FluteNoteA.swf", "NoteHolder2");
    tellTarget ("Flute/keyA") {
        gotoAndPlay("Play");
    }
    NoteTarget = "2";
    NotePlaying2 = "0";
    Loaded = "0";
} else {
    loadMovie("FluteNoteA.swf", "NoteHolder3");
    tellTarget ("Flute/keyA") {
        gotoAndPlay("Play");
    }
    NoteTarget = "0";
    NotePlaying3 = "0";
    Loaded = "0";
}
