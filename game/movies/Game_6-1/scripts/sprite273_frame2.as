#version 4
RandNum = random("2");
if (RandNum eq "1") {
    setProperty("green", _visible, "0");
    setProperty("orange", _visible, "1");
} else {
    setProperty("orange", _visible, "0");
    setProperty("green", _visible, "1");
}
stop();
