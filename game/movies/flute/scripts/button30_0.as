#version 4
SimonNotes = SimonNotes add "C";
if (NoteTarget eq "0") {
    loadMovie("FluteNoteC.swf", "NoteHolder");
    tellTarget ("Flute/keyC") {
        gotoAndPlay("Play");
    }
    NoteTarget = "1";
    NotePlaying1 = "0";
    Loaded = "0";
} else if (NoteTarget eq "1") {
    loadMovie("FluteNoteC.swf", "NoteHolder2");
    tellTarget ("Flute/keyC") {
        gotoAndPlay("Play");
    }
    NoteTarget = "2";
    NotePlaying2 = "0";
    Loaded = "0";
} else {
    loadMovie("FluteNoteC.swf", "NoteHolder3");
    tellTarget ("Flute/keyC") {
        gotoAndPlay("Play");
    }
    NoteTarget = "0";
    NotePlaying3 = "0";
    Loaded = "0";
}
