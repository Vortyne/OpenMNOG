#version 4
if ("1" < eval("/:cBarNum")) {
    cNum = cNum - "1";
    if (cNum < "1") {
        cNum = eval("/:cBarNum");
    }
    gotoAndPlay(2);
}
if (eval("TextBar/:AllowChange") eq "1" and eval("TextBar/:cTextOpen") eq "1") {
    tellTarget ("TextBar") {
        gotoAndPlay("Close cText");
    }
}
