#version 4
GaKoro_Cog_Placed = eval("../:GaKoro_Cog_Placed");
if (GaKoro_Cog_Placed eq "1") {
    Looped = Looped + "1";
    if ("5" < Looped) {
        tellTarget ("/") {
            MovieName = "Episode_3-3.swf";
            Sound_ChangeSFXMovie = "1";
            Sound_SFXMovie = "blank.swf";
            Sound_ChangeBackgroundMovie = "1";
            Sound_Background = "blank.swf";
            VerticalShift = "0";
            HorizontalShift = "0";
            BeginLoadMovie = "1";
        }
    }
    gotoAndPlay(3);
}
