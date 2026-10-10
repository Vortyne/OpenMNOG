#version 4
AllowChange = "1";
cBarOpen = "0";
if (eval("/:cBarNum") < "1" or "5" < eval("/:Quest")) {
    setProperty("cBar", _visible, "0");
}
gotoAndPlay(3);
