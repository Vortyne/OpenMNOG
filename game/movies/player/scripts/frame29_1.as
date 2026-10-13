#version 5
Movieisloaded = "0";
LoadStarted = getTimer();
AllowTextSub = "1";
BeginLoadMovie = "0";
if (BookOpen eq "0") {
    loadMovie(MovieName, "/holder");
    if (Sound_ChangeBackgroundMovie eq "1") {
        loadMovie(Sound_Background, "/Sound-Background");
        Sound_ChangeBackgroundMovie = "0";
        Sound_ChangeBackgroundVolume = "0";
    }
    if (Sound_ChangeSFXMovie eq "1") {
        loadMovie(Sound_SFXMovie, "/Sound-SFX");
        Sound_ChangeSFXMovie = "0";
    }
    if (Music_ChangeMusic eq "1") {
        loadMovie(Music_Movie, "/Music-holder");
        Music_ChangeMusic = "0";
    }
    if (Backpack eq "1") {
        tellTarget ("/Backpack") {
            gotoAndPlay("Closed");
        }
        tellTarget ("/Backpack/cBar") {
            gotoAndPlay("Closed");
        }
    }
} else {
    // (OpenMNOG: what's playing is what comes back when the Chronicle is closed, whatever the variables say: the
    // boatman's dialog in Ga-Koro, for one, leaves them empty, and closing it brought back nothing.)
    Sound_Background = getProperty("/Sound-Background", _url);
    Sound_SFXMovie = getProperty("/Sound-SFX", _url);
    Music_Movie = getProperty("/Music-holder", _url);
    loadMovie(BookMovieName, "/holder");
    loadMovie("blank.swf", "/Sound-Background");
    loadMovie("blank.swf", "/Sound-SFX");
    loadMovie("blank.swf", "/Music-holder");
    Sound_ChangeSFXMovie = "1";
    Music_ChangeMusic = "1";
    Sound_ChangeBackgroundMovie = "1";
    Sound_ChangeBackgroundVolume = "1";
}
