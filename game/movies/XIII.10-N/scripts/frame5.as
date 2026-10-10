#version 4
if (Brighten eq "1" and "0" < getProperty("Fore-Ground/Dark_FG", _alpha)) {
    setProperty("Fore-Ground/Dark_FG", _alpha, getProperty("Fore-Ground/Dark_FG", _alpha) - "5");
} else if (Brighten eq "0" and getProperty("Fore-Ground/Dark_FG", _alpha) < "40") {
    setProperty("Fore-Ground/Dark_FG", _alpha, getProperty("Fore-Ground/Dark_FG", _alpha) + "5");
}
if (Brighten eq "1" and "0" < getProperty("Middle-Ground/Dark_MG", _alpha)) {
    setProperty("Middle-Ground/Dark_MG", _alpha, getProperty("Middle-Ground/Dark_MG", _alpha) - "5");
} else if (Brighten eq "0" and getProperty("Middle-Ground/Dark_MG", _alpha) < "40") {
    setProperty("Middle-Ground/Dark_MG", _alpha, getProperty("Middle-Ground/Dark_MG", _alpha) + "5");
}
gotoAndPlay(4);
