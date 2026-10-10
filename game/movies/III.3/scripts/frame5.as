#version 4
LastDir = LastDir + "2";
if ("4" < LastDir) {
    LastDir = "1";
}
if (CurrentScreen eq KapuraIsAt && eval("/:cKapura") eq "0") {
    KapuraLocation = LastDir + "1";
    if ("4" < KapuraLocation) {
        KapuraLocation = "1";
    }
    tellTarget ("/holder/Middle-Ground/Panel" add KapuraLocation add "/Kapura") {
        gotoAndStop(2);
    }
}
XCount = "0";
while (XCount < "4") {
    if (eval("XLink" add XCount) eq "1") {
        XOn = XCount + "1";
        tellTarget ("/holder/Middle-Ground/Panel" add XOn add "/Marker") {
            gotoAndStop("Marked");
        }
    }
    XCount = XCount + "1";
}
Counter = "0";
while (Counter < PanelNum) {
    Counter = Counter + "1";
    setProperty("/holder/Fore-Ground/Panel" add LastDir, _y, "0");
    setProperty("/holder/Fore-Ground/Panel" add LastDir, _x, FGxOffset);
    FGxOffset = FGxOffset + FGWidth;
    setProperty("/holder/Middle-Ground/Panel" add LastDir, _y, "0");
    setProperty("/holder/Middle-Ground/Panel" add LastDir, _x, MGxOffset);
    MGxOffset = MGxOffset + MGWidth;
    setProperty("/holder/Back-Ground/Panel" add LastDir, _y, "0");
    setProperty("/holder/Back-Ground/Panel" add LastDir, _x, BGxOffset);
    BGxOffset = BGxOffset + BGWidth;
    LastDir = LastDir + "1";
    if ("4" < LastDir) {
        LastDir = "1";
    }
}
