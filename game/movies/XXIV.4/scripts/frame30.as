#version 4
count = "1";
while (count < eval("/:cBarNum")) {
    count = count + "1";
    cCheck = eval("/:Comrade_" add count);
    if (substring(cCheck, "9", "-1") eq "maku.swf") {
        set("/Backpack/cBar/:cLoadNum", count);
    }
}
tellTarget ("cBarOpen") {
    gotoAndPlay("Play");
}
