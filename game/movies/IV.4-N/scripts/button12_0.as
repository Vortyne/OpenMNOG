#version 4
setProperty("/holder/Back-Ground/Button-Castle", _x, "2000");
setProperty("/holder/Back-Ground/Gates-Shut", _x, "-69.1");
setProperty("/holder/Back-Ground/Gates-Shut", _y, "68.5");
tellTarget ("/") {
    MovieName = "IV.5-N.swf";
    VerticalShift = "0";
    HorizontalShift = "1";
    BeginLoadMovie = "1";
}
