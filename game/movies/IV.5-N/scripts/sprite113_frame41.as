#version 4
if (Walk eq "1") {
    if (getProperty("Surfer", _x) < "1340") {
        setProperty("Surfer", _x, getProperty("Surfer", _x) + "3.8");
    } else {
        setProperty("Surfer", _x, "-9.4");
        gotoAndPlay(1);
    }
}
