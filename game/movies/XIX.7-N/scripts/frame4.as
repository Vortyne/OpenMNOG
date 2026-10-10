#version 4
tellTarget ("/") {
    Movieisloaded = "1";
}
if ("4" < eval("/:Kokoro")) {
    count1 = "0";
    count3 = "6";
    NumLevels = "2";
    NumItems = "6";
    while (count1 < NumLevels) {
        count1 = count1 + "1";
        count2 = "0";
        while (count2 < NumItems) {
            count2 = count2 + "1";
            count3 = count3 + "1";
            setProperty("Level" add count1 add "-" add count2, _x, getProperty("Hole" add count3, _x));
            setProperty("Level" add count1 add "-" add count2, _y, getProperty("Hole" add count3, _y));
        }
    }
    gotoAndPlay(40);
} else {
    setProperty("Placed1-1", _x, getProperty("Hole7", _x));
    setProperty("Placed1-1", _y, getProperty("Hole7", _y));
    setProperty("Level1-1", _x, "2000");
    setProperty("Placed2-1", _x, getProperty("Hole13", _x));
    setProperty("Placed2-1", _y, getProperty("Hole13", _y));
    setProperty("Level2-1", _x, "2000");
    set("Placed-1-1", "1");
    set("Placed-2-1", "1");
    set("Hole7/:Holding", "1");
    set("Hole7/:LevelHolding", "1");
    set("Hole7/:ItemHolding", "1");
    set("Hole13/:Holding", "1");
    set("Hole13/:LevelHolding", "2");
    set("Hole13/:ItemHolding", "1");
}
