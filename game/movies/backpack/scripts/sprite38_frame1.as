#version 4
cBarNum = eval("/:cBarNum");
Name = "";
if ("0" < cBarNum) {
    cNum = "1";
    setProperty("../cBar", _visible, "1");
} else {
    cNum = "0";
    setProperty("../cBar", _visible, "0");
}
if ("0" < cNum) {
    if ("0" < eval("../:cLoadNum")) {
        loadMovie(eval("/:Comrade_" add eval("../:cLoadNum")), "cHolder");
        cNum = eval("../:cLoadNum");
    } else {
        loadMovie(eval("/:Comrade_" add cNum), "cHolder");
    }
}
gotoAndPlay(3);
