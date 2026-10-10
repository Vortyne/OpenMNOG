#version 4
cBarNum = "2";
if (eval("/:cBarNum") < "1" or "5" < eval("/:Quest")) {
    setProperty("cBar/cBar", _visible, "0");
}
