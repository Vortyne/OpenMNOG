#version 5
#operators flash4
Time_Passed = (currenttime - Time_Start) / "1000";
if (SFX_Length < currenttime - Time_SFXLoaded) {
    SongPicker = random("15");
    if ("5" < SongPicker) {
        Sound_SFX = "blank";
        SFX_Length = Sound_SilenceLength;
    } else if (SongPicker eq "4") {
        tellTarget ("/Sound-SFX") {
            gotoAndPlay("SFX_One");
        }
        Sound_SFX = SFX_One;
        SFX_Length = Sound_OneLength;
    } else if (SongPicker eq "3") {
        tellTarget ("/Sound-SFX") {
            gotoAndPlay("SFX_Two");
        }
        Sound_SFX = SFX_Two;
        SFX_Length = Sound_TwoLength;
    } else if (SongPicker eq "2") {
        tellTarget ("/Sound-SFX") {
            gotoAndPlay("SFX_Three");
        }
        Sound_SFX = SFX_Three;
        SFX_Length = Sound_ThreeLength;
    } else if (SongPicker eq "1") {
        tellTarget ("/Sound-SFX") {
            gotoAndPlay("SFX_Four");
        }
        Sound_SFX = SFX_Four;
        SFX_Length = Sound_FourLength;
    } else if (SongPicker eq "0") {
        tellTarget ("/Sound-SFX") {
            gotoAndPlay("SFX_Five");
        }
        Sound_SFX = SFX_Five;
        SFX_Length = Sound_FiveLength;
    }
    Time_SFXLoaded = currenttime;
}
if (BeginLoadMovie eq "1") {
    gotoAndPlay(24);
} else {
    gotoAndPlay(22);
}
