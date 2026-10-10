#version 5
if (GaKoro eq "1" and Book eq "0") {
    HadBook = "0";
}
if (Quest eq "7") {
    BookOpen = "1";
    loadMovie(BookMovieName, "/holder");
    loadMovie("blank.swf", "/Sound-Background");
    loadMovie("blank.swf", "/Sound-SFX");
    loadMovie("blank.swf", "/Music-holder");
} else {
    loadMovie(Sound_Background, "/Sound-Background");
    loadMovie(MovieName, "/holder");
    loadMovie(Sound_SFXMovie, "/Sound-SFX");
    loadMovie(Music_Movie, "/Music-holder");
}
gotoAndPlay(33);
