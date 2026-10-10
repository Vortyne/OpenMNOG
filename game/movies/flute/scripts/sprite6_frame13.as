#version 4
MovieName = eval("/:MovieName");
KewaFail = "0";
if (substring(Moviename, "0", "4") eq "XVI." or substring(Moviename, "0", "3") eq "XV." or substring(Moviename, "0", "4") eq "XIV.") {
    KewaFail = "1";
} else if (substring(Moviename, "0", "5") eq "XIII." or substring(Moviename, "0", "4") eq "XII." or substring(Moviename, "0", "3") eq "XI." or substring(Moviename, "0", "3") eq "VI.") {
    KewaFail = "1";
} else if (eval("/:KoKoro") < "7" and (substring(Moviename, "0", "4") eq "XIX." or substring(Moviename, "0", "3") eq "XX." or substring(Moviename, "0", "4") eq "XXI.")) {
    KewaFail = "1";
} else if ("4" < eval("/:Quest") and (substring(Moviename, "0", "6") eq "XXIII." or substring(Moviename, "0", "5") eq "XXIV." or substring(Moviename, "0", "5") eq "XXII.")) {
    KewaFail = "1";
} else {
    count = "0";
    while (count < NumScreen) {
        count = count + "1";
        if (eval("Screen" add count) eq MovieName) {
            KewaFail = "1";
        }
    }
}
if (KewaFail eq "1") {
    if (substring(Moviename, "0", "6") eq "XXIII." or substring(Moviename, "0", "5") eq "XXIV." or substring(Moviename, "0", "5") eq "XXII.") {
        getURL("FSCommand:message", "You should not leave this important task.");
    } else {
        getURL("FSCommand:message", "The Kewa cannot reach you here.");
    }
}
if (KewaFail eq "0") {
    tellTarget ("/") {
        MovieName = "KewaRide.swf";
        Sound_ChangeBackgroundMovie = "1";
        Sound_Background = "blank.swf";
        Sound_ChangeSFXMovie = "1";
        Sound_SFXMovie = "blank.swf";
        Music_ChangeMusic = "1";
        Music_Movie = "blank.swf";
        ShowBackPack = "0";
        BeginLoadMovie = "1";
    }
}
gotoAndPlay(21);
