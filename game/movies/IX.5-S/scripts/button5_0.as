#version 4
setTarget("/");
QuarryKeyUsed = eval("/:QuarryKeyUsed");
if (QuarryKeyUsed eq "1") {
    MovieName = "IX.4E.swf";
} else if (QuarryKeyUsed eq "2") {
    MovieName = "IX.4C.swf";
} else if (QuarryKeyUsed eq "3") {
    MovieName = "IX.4D.swf";
} else if (QuarryKeyUsed eq "4") {
    MovieName = "IX.4A.swf";
} else if (QuarryKeyUsed eq "5") {
    MovieName = "IX.4F.swf";
} else if (QuarryKeyUsed eq "6") {
    MovieName = "IX.4B.swf";
}
Sound_ChangeBackgroundMovie = "1";
Sound_Background = "Amb_wind.swf";
Sound_ChangeSFXMovie = "1";
Sound_SFXMovie = "Sound_Desert.swf";
BeginLoadMovie = "1";
setTarget("");
