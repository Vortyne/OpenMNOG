#version 4
SimonNotes = SimonNotes add "B";
if (NoteTarget eq "0") {
    loadMovie("FluteNoteB.swf", "NoteHolder");
    tellTarget ("Flute/keyB") {
        gotoAndPlay("Play");
    }
    NoteTarget = "1";
    NotePlaying1 = "0";
    Loaded = "0";
} else if (NoteTarget eq "1") {
    loadMovie("FluteNoteB.swf", "NoteHolder2");
    tellTarget ("Flute/keyB") {
        gotoAndPlay("Play");
    }
    NoteTarget = "2";
    NotePlaying2 = "0";
    Loaded = "0";
} else {
    loadMovie("FluteNoteB.swf", "NoteHolder3");
    tellTarget ("Flute/keyB") {
        gotoAndPlay("Play");
    }
    NoteTarget = "0";
    NotePlaying3 = "0";
    Loaded = "0";
}
