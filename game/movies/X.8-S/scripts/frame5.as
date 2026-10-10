#version 4
if (Brighten eq "1" and "0" < getProperty("Darkness", _alpha)) {
    setProperty("Darkness", _alpha, getProperty("Darkness", _alpha) - "5");
} else if (Brighten eq "0" and getProperty("Darkness", _alpha) < "60") {
    setProperty("Darkness", _alpha, getProperty("Darkness", _alpha) + "5");
}
gotoAndPlay(4);
