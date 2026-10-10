#version 4
KohliBallUsed = eval("/:KohliBallUsed");
BookOpen = eval("/:BookOpen");
if (BookOpen eq "0") {
    if (KohliBallUsed eq "0") {
        tellTarget ("/") {
            MovieName = "IX.6-N.swf";
            VerticalShift = "0";
            HorizontalShift = "0";
            ShiftOff = "1";
            ShowBackPack = "1";
            BeginLoadMovie = "1";
        }
    }
}
