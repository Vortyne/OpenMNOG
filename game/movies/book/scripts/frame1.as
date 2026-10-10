#version 4
TextHeight = "45";
Shifting = "0";
// The list moves this far a frame (it was 9: a slot took 5 frames, now 3), and the clicks waiting their turn.
ShiftStep = "15";
QueuedUp = "0";
QueuedDown = "0";
tellTarget ("TextSelect") {
    TextSlot1 = "Mata Nui 1";
    TextSlot2 = "Tahu 1";
    TextSlot3 = "Tahu 2";
    TextSlot4 = "Nokama 1";
    TextSlot5 = "Gali 1";
    TextSlot6 = "Pohatu 1";
    TextSlot7 = "Pohatu 2";
    TextSlot8 = "LeKoro 1";
    TextSlot9 = "LeKoro 2";
    TextSlot10 = "LeKoro 3";
    TextSlot11 = "LeKoro 4";
    TextSlot12 = "LeKoro 5";
    TextSlot13 = "KoKoro 1";
    TextSlot14 = "p8 1";
    TextSlot15 = "p8 2";
    TextSlot16 = "p8 3";
    TextSlot17 = "p8 4";
    TextSlot18 = "p8 5";
    TextSlot19 = "p8 6";
    TextSlot20 = "p8 7";
    TextSlot21 = "p8 8";
    TextSlot22 = "p9 1";
    TextSlot23 = "p9 2";
    TextSlot24 = "p9 3";
    TextSlot25 = "p9 6";
}
TextSlot1 = "MataNui-01";
TextSlot2 = "Tahu-01";
TextSlot3 = "Tahu-02";
TextSlot4 = "Nokama-01";
TextSlot5 = "Gali-01";
TextSlot6 = "Pohatu-01";
TextSlot7 = "Pohatu-02";
TextSlot8 = "LeKoro-01";
TextSlot9 = "LeKoro-02";
TextSlot10 = "LeKoro-03";
TextSlot11 = "LeKoro-04";
TextSlot12 = "LeKoro-05";
TextSlot13 = "Kopaka-01";
TextSlot14 = "Maku";
TextSlot15 = "Tamaru";
TextSlot16 = "Hafu-Taipu";
TextSlot17 = "Kopeke";
TextSlot18 = "Kini-Nui";
TextSlot19 = "Toa-Kaita-01";
TextSlot20 = "Toa-Kaita-02";
TextSlot21 = "Toa-Kaita-03";
TextSlot22 = "Toa-Kaita-04";
TextSlot23 = "Takua";
TextSlot24 = "Makuta";
TextSlot25 = "Vakama";
MovieSlot1 = "Episode_1-1.swf";
MovieSlot2 = "Episode_1-2.swf";
MovieSlot3 = "Episode_2-1.swf";
MovieSlot4 = "Episode_3-2.swf";
MovieSlot5 = "Episode_3-3.swf";
MovieSlot6 = "Episode_4-2.swf";
MovieSlot7 = "Episode_4-3.swf";
MovieSlot8 = "Episode_6-2.swf";
MovieSlot9 = "Episode_6-3.swf";
MovieSlot10 = "Episode_6-4.swf";
MovieSlot11 = "Episode_6-5.swf";
MovieSlot12 = "Episode_6-7.swf";
MovieSlot13 = "Episode_7-2.swf";
MovieSlot14 = "Episode_8-1.swf";
MovieSlot15 = "Episode_8-2.swf";
MovieSlot16 = "Episode_8-3.swf";
MovieSlot17 = "Episode_8-4.swf";
MovieSlot18 = "Episode_8-5.swf";
MovieSlot19 = "Episode_8-6.swf";
MovieSlot20 = "Episode_8-7.swf";
MovieSlot21 = "Episode_8-8.swf";
MovieSlot22 = "Episode_9-1.swf";
MovieSlot23 = "Episode_9-2.swf";
MovieSlot24 = "Episode_9-3.swf";
MovieSlot25 = "Episode_9-6.swf";
CurrentTextSlot = "1";
MaxSlots = "7";
LeKoro = "1";
LeKoro = eval("/:LeKoro");
if ("1" < LeKoro) {
    MaxSlots = MaxSlots + "1";
    if ("2" < LeKoro) {
        MaxSlots = MaxSlots + "1";
        if ("3" < LeKoro) {
            MaxSlots = MaxSlots + "1";
            if ("4" < LeKoro) {
                MaxSlots = MaxSlots + "2";
                if (eval("/:KoKoro") eq "7") {
                    MaxSlots = MaxSlots + "1";
                    if ("0" < eval("/:Quest")) {
                        MaxSlots = MaxSlots + "1";
                        if ("1" < eval("/:Quest")) {
                            MaxSlots = MaxSlots + "1";
                            if ("2" < eval("/:Quest")) {
                                MaxSlots = MaxSlots + "1";
                                if ("3" < eval("/:Quest")) {
                                    MaxSlots = MaxSlots + "1";
                                    if ("4" < eval("/:Quest")) {
                                        MaxSlots = MaxSlots + "1";
                                        if ("0" < eval("/:FightVision")) {
                                            MaxSlots = MaxSlots + "1";
                                            if ("1" < eval("/:FightVision")) {
                                                MaxSlots = MaxSlots + "1";
                                                if ("2" < eval("/:FightVision")) {
                                                    MaxSlots = MaxSlots + "1";
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
if (eval("/:Quest") eq "7") {
    MaxSlots = "25";
}
// Opened again (on its first frame, so the list is placed there at once, not slid there): at the slot whose video was played last (the player keeps it, ChronicleSlot), the list moved there.
if (Restored ne "1") {
    Restored = "1";
    LastSlot = eval("/:ChronicleSlot");
    if ("1" < LastSlot) {
        if (LastSlot <= MaxSlots) {
            CurrentTextSlot = LastSlot;
            setProperty("TextSelect", _y, getProperty("TextSelect", _y) - (LastSlot - 1) * TextHeight);
            tellTarget ("Thumbnails") {
                DoFadein = "1";
            }
        }
    }
}
tellTarget ("/") {
    Movieisloaded = "1";
}
tellTarget ("/mousedrag") {
    gotoAndPlay("mouseblank");
}
