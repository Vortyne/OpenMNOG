#version 4
// (OpenMNOG: once the lava is fixed, Onu-Koro's second music plays here, carrying on from the last scene if it was
// already playing; before that, never: Onu-Koro's first music, also when arriving with none, e.g. a save made after
// warping here.)
if (eval("/:OnuKoroLava") eq "1") {
    if (getProperty("/Music-holder", _url) ne "Music_OnuKoro_2.swf") {
        tellTarget ("/") {
            Music_Movie = "Music_OnuKoro_2.swf";
            Music_ChangeMusic = "0";
        }
        loadMovie("Music_OnuKoro_2.swf", "/Music-holder");
    }
} else if (getProperty("/Music-holder", _url) ne "Music_OnuKoro.swf") {
    tellTarget ("/") {
        Music_Movie = "Music_OnuKoro.swf";
        Music_ChangeMusic = "0";
    }
    loadMovie("Music_OnuKoro.swf", "/Music-holder");
}
// (OpenMNOG: arriving with no background sound (a save made after warping here), Onu-Koro's cave ambience, as the
// tunnels in from Po-Wahi set it: any other ambience it arrives with is kept.)
if (getProperty("/Sound-Background", _url) eq "blank.swf") {
    tellTarget ("/") {
        Sound_Background = "Amb_tunnel.swf";
        Sound_ChangeBackgroundMovie = "0";
        Sound_SFXMovie = "Sound_Caves.swf";
        Sound_ChangeSFXMovie = "0";
    }
    loadMovie("Amb_tunnel.swf", "/Sound-Background");
    loadMovie("Sound_Caves.swf", "/Sound-SFX");
}
tellTarget ("/") {
    Startedloading = "1";
}
Count = "0";
Language = eval("/:Language");
TextToLoad = "Text_XI5A_Kapura_" add Language add ".txt";
